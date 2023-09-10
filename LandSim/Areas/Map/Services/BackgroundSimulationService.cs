using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map.Services
{
    public class BackgroundSimulationService : BackgroundService
    {
        private readonly ILogger<BackgroundSimulationService> _logger;
        private readonly IServiceScopeFactory _services;
        private readonly SimulationEventAggregator _eventAggregator;

        public BackgroundSimulationService(ILogger<BackgroundSimulationService> logger, IServiceScopeFactory services, SimulationEventAggregator eventAggregator)
        {
            _logger = logger;
            _services = services;
            _eventAggregator = eventAggregator;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using(var scope = _services.CreateScope())
            {
                var mapRepository = scope.ServiceProvider.GetRequiredService<MapGenerationRepository>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Running background sim loop");

                    var random = new Random();

                    var currentTiles = mapRepository.GetTerrain();
                    var updatedTiles = new TerrainTile[currentTiles.GetLength(0), currentTiles.GetLength(1)];
                    
                    for(var x = 0; x < currentTiles.GetLength(0); x++)
                    {
                        for (var y = 0; y < currentTiles.GetLength(1); y++)
                        {
                            var updatedTile = currentTiles[x, y]?.Clone();

                            if (updatedTile == null)
                            {
                                continue;
                            }

                            if (updatedTile.TerrainType == TerrainType.Soil)
                            {
                                var surroundingTiles = currentTiles.GetImmediateNeighbors(x, y);
                                
                                if (random.NextDouble() >= 0.9999)
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }

                                if(random.NextDouble() >= 0.9 && surroundingTiles.Any(t => t.VegetationLevel > 0))
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }

                                if (updatedTile.VegetationLevel > 0)
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }
                            }

                            updatedTiles[x, y] = updatedTile;
                        }
                    }
                    
                    // TODO: Couldn't figure out how to do this by just returning the updated tiles, so need to track the changes by updating the existing tiles.
                    // Should try to find a solution that doesn't involve two loops
                    for (var x = 0; x < currentTiles.GetLength(0); x++)
                    {
                        for (var y = 0; y < currentTiles.GetLength(1); y++)
                        {
                            var existingTile = currentTiles[x, y];
                            var updatedTile = updatedTiles[x, y];

                            if (existingTile == null || updatedTile == null)
                            {
                                continue;
                            }

                            existingTile.VegetationLevel = updatedTile.VegetationLevel;
                            existingTile.TerrainType = updatedTile.TerrainType;
                        }
                    }

                    mapRepository.SaveTerrain(currentTiles);

                    _eventAggregator.Publish(new MapUpdateEvent { TerrainTiles = currentTiles });

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
