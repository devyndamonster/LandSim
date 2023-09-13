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
        private readonly SimulationService _simulationService;

        public BackgroundSimulationService(
            ILogger<BackgroundSimulationService> logger,
            IServiceScopeFactory services,
            SimulationEventAggregator eventAggregator,
            SimulationService simulationService)
        {
            _logger = logger;
            _services = services;
            _eventAggregator = eventAggregator;
            _simulationService = simulationService;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using(var scope = _services.CreateScope())
            {
                var mapRepository = scope.ServiceProvider.GetRequiredService<MapGenerationRepository>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Running background sim loop");
                    
                    var currentWorldData = mapRepository.GetWorldData();
                    var simulationUpdates = _simulationService.GetSimulationUpdates(currentWorldData);
                    
                    var locationToTileUpdates = simulationUpdates.UpdatedTiles
                        .ToDictionary(tile => (x: tile?.XCoord, y: tile?.YCoord), tile => tile);

                    var updatedTerrainGrid = currentWorldData.TerrainTiles
                        .Map(tile =>
                        {
                            if (locationToTileUpdates.TryGetValue((tile?.XCoord, tile?.YCoord), out var updatedTile))
                            {
                                return updatedTile;
                            }

                            return tile;
                        });

                    mapRepository.SaveTerrain(currentWorldData.TerrainTiles);
                    //mapRepository.AddConsumables(simulationUpdates.AddedConsumables);
                    
                    //TODO: add consumables
                    _eventAggregator.Publish(new MapUpdateEvent { TerrainTiles = updatedTerrainGrid, Consumables = new Consumable[0,0] });

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
