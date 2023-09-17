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
                    
                    var updatedWorldData = _simulationService.GetUpdatedWorldData(currentWorldData, new AgentAction[0]);
                    _logger.LogInformation($"Got Updated World - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    var simulationUpdates = _simulationService.GetSimulationUpdates(currentWorldData, updatedWorldData);
                    _logger.LogInformation($"Got Updates From World Data - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");
                    
                    await mapRepository.SaveSimulationUpdates(simulationUpdates);
                    _logger.LogInformation($"Saved Terrain - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");
                    
                    _eventAggregator.Publish(new MapUpdateEvent { 
                        TerrainTiles = updatedWorldData.TerrainTiles, 
                        Consumables = updatedWorldData.Consumables, 
                        Agents = updatedWorldData.Agents
                    });

                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }
}
