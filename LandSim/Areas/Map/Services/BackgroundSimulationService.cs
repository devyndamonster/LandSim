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

                    var currentWorldData = mapRepository.GetWorldData();
                    var updatedWorldData = currentWorldData.Clone();

                    for (var x = 0; x < currentWorldData.Bounds.SizeX; x++)
                    {
                        for (var y = 0; y < currentWorldData.Bounds.SizeY; y++)
                        {
                            var currentTile = currentWorldData.TerrainTiles[x, y];
                            var updatedTile = updatedWorldData.TerrainTiles[x, y];

                            if (updatedTile == null || currentTile == null)
                            {
                                continue;
                            }

                            if (currentTile.TerrainType == TerrainType.Soil)
                            {
                                var surroundingTiles = currentWorldData.TerrainTiles.GetImmediateNeighbors(x, y);
                                
                                if (random.NextDouble() >= 0.9999)
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }

                                if(random.NextDouble() >= 0.9 && surroundingTiles.Any(t => t.VegetationLevel > 0))
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }

                                if (currentTile.VegetationLevel > 0)
                                {
                                    updatedTile.VegetationLevel += 0.01f;
                                }
                                
                                if (currentTile.VegetationLevel > 0.95 && random.NextDouble() >= 0.99 && currentWorldData.Consumables[x,y] == null)
                                {
                                    updatedWorldData.Consumables[x, y] = new Consumable
                                    {
                                        XCoord = currentTile.XCoord,
                                        YCoord = currentTile.YCoord,
                                    };
                                }
                            }

                            updatedWorldData.TerrainTiles[x, y] = updatedTile;
                        }
                    }

                    // TODO: Couldn't figure out how to do this by just returning the updated tiles, so need to track the changes by updating the existing tiles.
                    // Should try to find a solution that doesn't involve two loops

                    var removedConsumables = new List<Consumable>();
                    var addedConsumables = new List<Consumable>();

                    for (var x = 0; x < currentWorldData.Bounds.SizeX; x++)
                    {
                        for (var y = 0; y < currentWorldData.Bounds.SizeY; y++)
                        {
                            var existingTile = currentWorldData.TerrainTiles[x, y];
                            var updatedTile = updatedWorldData.TerrainTiles[x, y];

                            if (existingTile == null || updatedTile == null)
                            {
                                continue;
                            }

                            existingTile.VegetationLevel = updatedTile.VegetationLevel;
                            existingTile.TerrainType = updatedTile.TerrainType;

                            
                            if (currentWorldData.Consumables[x, y] != null && updatedWorldData.Consumables[x, y] == null)
                            {
                                removedConsumables.Add(currentWorldData.Consumables[x, y]!);
                            }
                            else if(currentWorldData.Consumables[x, y] == null && updatedWorldData.Consumables[x, y] != null)
                            {
                                addedConsumables.Add(updatedWorldData.Consumables[x, y]!);
                            }
                        }
                    }

                    mapRepository.SaveTerrain(currentWorldData.TerrainTiles);
                    mapRepository.RemoveConsumables(removedConsumables);
                    mapRepository.AddConsumables(addedConsumables);

                    _eventAggregator.Publish(new MapUpdateEvent { TerrainTiles = updatedWorldData.TerrainTiles, Consumables = updatedWorldData.Consumables });

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
