using LandSim.Areas.Agents;
using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Database;
using LandSim.Extensions;
using System.Diagnostics;

namespace LandSim.Areas.Simulation.Services
{
    public class BackgroundSimulationService : BackgroundService
    {
        private readonly ILogger<BackgroundSimulationService> _logger;
        private readonly IServiceScopeFactory _services;
        private readonly SimulationEventAggregator _eventAggregator;
        private readonly SimulationService _simulationService;
        private readonly AgentUpdateService _agentUpdateService;

        public BackgroundSimulationService(
            ILogger<BackgroundSimulationService> logger,
            IServiceScopeFactory services,
            SimulationEventAggregator eventAggregator,
            SimulationService simulationService,
            AgentUpdateService agentUpdateService)
        {
            _logger = logger;
            _services = services;
            _eventAggregator = eventAggregator;
            _simulationService = simulationService;
            _agentUpdateService = agentUpdateService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _services.CreateScope())
            {
                var mapRepository = scope.ServiceProvider.GetRequiredService<MapRepository>();
                var stopWatch = new Stopwatch();

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Running background sim loop - {DateTime.Now}");
                    stopWatch.Restart();

                    var currentWorldData = await mapRepository.GetWorldData();
                    var agentAction = await mapRepository.PopAgentActions();
                    var owners = await mapRepository.GetAgentOwners();
                    var config = (await mapRepository.GetConfigs()).FirstOrDefault(new SimulationConfig());
                    _logger.LogInformation($"Retrieved World Data - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    var updatedWorldData = _simulationService.GetUpdatedWorldData(currentWorldData, config, owners, agentAction);
                    _logger.LogInformation($"Got Updated World - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    var simulationUpdates = _simulationService.GetSimulationUpdates(currentWorldData, updatedWorldData);
                    _logger.LogInformation($"Got Updates From World Data - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    await mapRepository.SaveSimulationUpdates(simulationUpdates);
                    _logger.LogInformation($"Saved Terrain - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    var updateEvent = new MapUpdateEvent
                    {
                        TerrainTiles = updatedWorldData.TerrainTiles,
                        Consumables = updatedWorldData.Consumables,
                        Agents = updatedWorldData.Agents
                    };

                    _eventAggregator.Publish(updateEvent);
                    await _agentUpdateService.SendSimulationUpdate(updateEvent);
                    _logger.LogInformation($"Published Updates - {stopWatch.GetElapsedMillisecondsAndRestart()}ms");

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
