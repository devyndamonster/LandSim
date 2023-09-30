using FluentAssertions;
using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Services;
using LandSim.Extensions;

namespace LandSim.UnitTests.Areas.Simulation.Services
{
    public class SimulationServiceTests
    {
        public class GetUpdatedWorldData
        {
            [Fact]
            public void AgentsWontExistOnSameTile_WhenBothAgentsMoveToSameTile()
            {
                var terrain = new TerrainTile[]
                {
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 0, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 1, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 2, YCoord = 0 },
                };

                var agents = new Agent[]
                {
                    new Agent { AgentId = 1, XCoord = 0, YCoord = 0 },
                    new Agent { AgentId = 2, XCoord = 2, YCoord = 0 },
                };

                var consumables = new Consumable[0];

                var worldData = new WorldData(terrain, consumables, agents);

                var agentActions = new AgentAction[]
                {
                    new AgentAction{ AgentId = 1, ActionType = AgentActionType.MoveRight },
                    new AgentAction{ AgentId = 2, ActionType = AgentActionType.MoveLeft },
                };

                var agentOwners = new AgentOwner[]
                {
                    new AgentOwner { AgentOwnerId = 1 },
                };

                var simulationService = new SimulationService();

                var result = simulationService.GetUpdatedWorldData(worldData, new SimulationConfig(), agentOwners, agentActions);

                var agentPositions = result.Agents.Where(agent => agent.Value is not null).Select(agent => (agent.Value.XCoord, agent.Value.YCoord));
                agentPositions.Should().HaveCount(2);
                agentPositions.Should().OnlyHaveUniqueItems();
            }

            [Fact]
            public void AgentsWontExistOnSameTile_WhenOneMovesIntoAnother()
            {
                var terrain = new TerrainTile[]
                {
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 0, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 1, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 2, YCoord = 0 },
                };

                var agents = new Agent[]
                {
                    new Agent { AgentId = 1, XCoord = 0, YCoord = 0 },
                    new Agent { AgentId = 2, XCoord = 1, YCoord = 0 },
                };

                var consumables = new Consumable[0];

                var worldData = new WorldData(terrain, consumables, agents);

                var agentActions = new AgentAction[]
                {
                    new AgentAction{ AgentId = 1, ActionType = AgentActionType.MoveRight },
                };

                var agentOwners = new AgentOwner[]
                {
                    new AgentOwner { AgentOwnerId = 1 },
                };

                var simulationService = new SimulationService();

                var result = simulationService.GetUpdatedWorldData(worldData, new SimulationConfig(), agentOwners, agentActions);

                var agentPositions = result.Agents.Where(agent => agent.Value is not null).Select(agent => (agent.Value.XCoord, agent.Value.YCoord));
                agentPositions.Should().HaveCount(2);
                agentPositions.Should().OnlyHaveUniqueItems();
            }

            [Fact]
            public void AgentsWontExistOnSameTile_WhenOneMovesIntoOther_AndOthersMovementCancelled()
            {
                var terrain = new TerrainTile[]
                {
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 0, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Sand, XCoord = 1, YCoord = 0 },
                    new TerrainTile { TerrainType = TerrainType.Water, XCoord = 2, YCoord = 0 },
                };

                var agents = new Agent[]
                {
                    new Agent { AgentId = 1, XCoord = 0, YCoord = 0 },
                    new Agent { AgentId = 2, XCoord = 1, YCoord = 0 },
                };

                var consumables = new Consumable[0];

                var worldData = new WorldData(terrain, consumables, agents);

                var agentActions = new AgentAction[]
                {
                    new AgentAction{ AgentId = 1, ActionType = AgentActionType.MoveRight },
                    new AgentAction{ AgentId = 2, ActionType = AgentActionType.MoveRight },
                };

                var agentOwners = new AgentOwner[]
                {
                    new AgentOwner { AgentOwnerId = 1 },
                };

                var simulationService = new SimulationService();

                var result = simulationService.GetUpdatedWorldData(worldData, new SimulationConfig(), agentOwners, agentActions);

                var agentPositions = result.Agents.Where(agent => agent.Value is not null).Select(agent => (agent.Value.XCoord, agent.Value.YCoord));
                agentPositions.Should().HaveCount(2);
                agentPositions.Should().OnlyHaveUniqueItems();
            }
        }
    }
}