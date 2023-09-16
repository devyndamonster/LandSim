using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
                var stopWatch = new Stopwatch();

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Running background sim loop - {DateTime.Now}");
                    stopWatch.Restart();
                    
                    var currentWorldData = await mapRepository.GetWorldData();
                    _logger.LogInformation($"Retrieved World Data - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");
                    
                    var simulationUpdates = _simulationService.GetWorldSimulationUpdates(currentWorldData);
                    _logger.LogInformation($"Got Simulation Updates - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    var locationToTileUpdates = simulationUpdates.UpdatedTiles
                        .ToDictionary(tile => (x: tile?.XCoord, y: tile?.YCoord), tile => tile);

                    var locationToAddedConsumables = simulationUpdates.AddedConsumables
                        .ToDictionary(consumable => (x: consumable?.XCoord, y: consumable?.YCoord), consumable => consumable);

                    var updatedTerrainGrid = currentWorldData.TerrainTiles
                        .Map(tile =>
                        {
                            if (locationToTileUpdates.TryGetValue((tile.Value?.XCoord, tile.Value?.YCoord), out var updatedTile))
                            {
                                return updatedTile;
                            }

                            return tile.Value;
                        });

                    var updatedConsumablesGrid = currentWorldData.Consumables
                        .Map(consumable =>
                        {
                            if (consumable.Value is null)
                            {
                                return null;
                            }

                            if (locationToAddedConsumables.TryGetValue((consumable.Value?.XCoord, consumable.Value?.YCoord), out var addedConsumable))
                            {
                                return addedConsumable;
                            }

                            return consumable.Value;
                        });
                    _logger.LogInformation($"Mapped Updates To Grid - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");
                    
                    await mapRepository.SaveSimulationUpdates(simulationUpdates);
                    _logger.LogInformation($"Saved Terrain - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");
                    
                    _eventAggregator.Publish(new MapUpdateEvent { TerrainTiles = updatedTerrainGrid, Consumables = updatedConsumablesGrid });

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
