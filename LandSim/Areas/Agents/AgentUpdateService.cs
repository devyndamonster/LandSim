using LandSim.Areas.Agents.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Database;
using LandSim.Extensions;

namespace LandSim.Areas.Agents
{
    public class AgentUpdateService
    {
        private readonly AgentOwnerClient _agentClient;
        private readonly MapRepository _repository;

        public AgentUpdateService(AgentOwnerClient agentClient, MapRepository repository)
        {
            _agentClient = agentClient;
            _repository = repository;
        }

        public async Task SendSimulationUpdate(MapUpdateEvent mapUpdate)
        {
            var simulationConfig = (await _repository.GetConfigs()).FirstOrDefault();

            var agentContextUpdates = mapUpdate.Agents
                .Where(agent => agent.Value is not null)
                .Select(agent => new AgentContext
                {
                    Agent = agent.Value! with
                    {
                        XCoord = 0,
                        YCoord = 0,
                    },
                    TerrainTiles = mapUpdate.TerrainTiles
                        .GetElementsWithinRange(agent.x, agent.y, 5)
                        .MakeRelative(agent.Value!.XCoord, agent.Value.YCoord),
                    Agents = mapUpdate.Agents
                        .GetElementsWithinRange(agent.x, agent.y, 5)
                        .MakeRelative(agent.Value.XCoord, agent.Value.YCoord),
                    Consumables = mapUpdate.Consumables
                        .GetElementsWithinRange(agent.x, agent.y, 5)
                        .MakeRelative(agent.Value.XCoord, agent.Value.YCoord),
                    SimulationConfig = simulationConfig,
                })
                .GroupBy(agentContext => agentContext.Agent!.AgentOwnerId);

            var agentOwners = await _repository.GetAgentOwners();

            foreach (var agentContextUpdate in agentContextUpdates)
            {
                var owner = agentOwners.FirstOrDefault(owner => owner.AgentOwnerId == agentContextUpdate.Key);

                if(owner is not null)
                {
                    await _agentClient.SendSimulationUpdate(owner.PostbackUrl, agentContextUpdate);
                }
            }
        }
    }
}
