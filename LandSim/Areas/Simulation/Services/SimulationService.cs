using LandSim.Areas.Agents.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Extensions;

namespace LandSim.Areas.Simulation.Services
{
    public class SimulationService
    {
        public WorldData GetUpdatedWorldData(WorldData currentWorldData, IEnumerable<AgentOwner> agentOwners, IEnumerable<AgentAction> actions)
        {
            var random = new Random();

            var agentActions = actions.ToDictionary(action => action.AgentId);

            var updatedAgentGrid = currentWorldData.Agents
                .Select(agent =>
                {
                    if (agent.Value is null)
                    {
                        return null;
                    }

                    agentActions.TryGetValue(agent.Value.AgentId, out var action);

                    if (action is null)
                    {
                        action = new AgentAction
                        {
                            AgentId = agent.Value.AgentId,
                            ActionType = AgentActionType.None,
                        };
                    }

                    var destination = action.ActionType switch
                    {
                        AgentActionType.MoveLeft => (
                            tile: currentWorldData.TerrainTiles.Left(agent.x, agent.y),
                            agent: currentWorldData.Agents.Left(agent.x, agent.y)),
                        AgentActionType.MoveRight => (
                            tile: currentWorldData.TerrainTiles.Right(agent.x, agent.y),
                            agent: currentWorldData.Agents.Right(agent.x, agent.y)),
                        AgentActionType.MoveUp => (
                            tile: currentWorldData.TerrainTiles.Up(agent.x, agent.y),
                            agent: currentWorldData.Agents.Up(agent.x, agent.y)),
                        AgentActionType.MoveDown => (
                            tile: currentWorldData.TerrainTiles.Down(agent.x, agent.y),
                            agent: currentWorldData.Agents.Down(agent.x, agent.y)),
                        _ => (
                            tile: currentWorldData.TerrainTiles[agent.x, agent.y],
                            agent: currentWorldData.Agents[agent.x, agent.y]),
                    };

                    Func<Agent> getUpdatedAgent = action.ActionType switch
                    {
                        AgentActionType.Eat => () =>
                        {
                            var consumable = currentWorldData.Consumables[agent.x, agent.y];
                            var hungerDelta = consumable is not null ? 0.1f : 0;

                            return agent.Value with
                            {
                                Hunger = agent.Value.Hunger + hungerDelta,
                            };
                        },
                        AgentActionType.MoveLeft or AgentActionType.MoveUp or AgentActionType.MoveRight or AgentActionType.MoveDown 
                        when destination.tile is not null 
                            && destination.tile.TerrainType is not TerrainType.Water 
                            && destination.agent is null => () =>
                        {
                            var currentTile = currentWorldData.TerrainTiles[agent.x, agent.y]!;
                            var heightIncrease = MathF.Max(0, destination.tile.Height - currentTile.Height);

                            return agent.Value with
                            {
                                XCoord = destination.tile.XCoord,
                                YCoord = destination.tile.YCoord,
                                Hunger = agent.Value.Hunger - (0.02f + heightIncrease),
                            };
                        },
                        _ => () => agent.Value with
                        {
                            Hunger = agent.Value.Hunger - 0.01f
                        }
                    };

                    var updatedAgent = getUpdatedAgent();

                    return updatedAgent.Hunger > 0 ? updatedAgent : null;
                })
                .OfType<Agent>()
                .MapLocationsToBoundedGrid(currentWorldData.Bounds);
            
            //New Agents
            updatedAgentGrid = updatedAgentGrid.Map(agent =>
            {
                if (agent.Value is null)
                {
                    var tile = currentWorldData.TerrainTiles[agent.x, agent.y];

                    if (tile?.TerrainType == TerrainType.Sand && random.NextDouble() >= 0.9999)
                    {
                        return new Agent
                        {
                            XCoord = tile.XCoord,
                            YCoord = tile.YCoord,
                            AgentOwnerId = agentOwners.Shuffle().First().AgentOwnerId,
                            Hunger = 1,
                            Thirst = 1,
                        };
                    }
                }

                return agent.Value;
            });

            var updatedTilesGrid = currentWorldData.TerrainTiles.Map(tile =>
            {
                var surroundingTiles = currentWorldData.TerrainTiles.GetImmediateNeighbors(tile.x, tile.y);

                return tile switch
                {
                    { Value.TerrainType: TerrainType.Soil } => GetVegetationUpdate(tile.Value, surroundingTiles),
                    _ => tile.Value
                };
            });

            var updatedConsumablesGrid = currentWorldData.Consumables.Map(consumable =>
            {
                var terrainTile = updatedTilesGrid[consumable.x, consumable.y];
                var agentOnTile = updatedAgentGrid[consumable.x, consumable.y];
                var agentAction = agentActions.GetValueOrDefault(agentOnTile?.AgentId ?? 0)?.ActionType ?? AgentActionType.None;

                return consumable switch
                {
                    var c when c.Value is not null && agentAction == AgentActionType.Eat => null,
                    var c when c.Value is null
                        && terrainTile?.VegetationLevel > 0.95
                        && random.NextDouble() >= 0.999 =>
                            new Consumable { XCoord = terrainTile.XCoord, YCoord = terrainTile.YCoord },
                    var c => c.Value
                };
            });

            return new WorldData(currentWorldData.Bounds, updatedTilesGrid!, updatedConsumablesGrid!, updatedAgentGrid!);
        }

        public SimulationUpdates GetSimulationUpdates(WorldData currentWorld, WorldData updatedWorld)
        {
            var currentAgents = currentWorld.Agents
                .Select(agent => agent.Value)
                .OfType<Agent>()
                .ToDictionary(agent => agent.AgentId);

            var updatedAgents = updatedWorld.Agents
                .Select(agent => agent.Value)
                .OfType<Agent>()
                .ToDictionary(agent => agent.AgentId);

            var agentUpdates = updatedAgents.Values
                .Where(agent =>
                {
                    if (currentAgents.TryGetValue(agent.AgentId, out var currentAgent))
                    {
                        return !agent.Equals(currentAgent);
                    }

                    return false;
                });

            var agentAdditions = updatedAgents.Values
                .Where(agent => !currentAgents.ContainsKey(agent.AgentId));

            var agentRemovals = currentAgents.Values
                .Where(agent => !updatedAgents.ContainsKey(agent.AgentId));

            return new SimulationUpdates
            {
                UpdatedTiles = updatedWorld.TerrainTiles.Updates(currentWorld.TerrainTiles).ToList(),
                AddedConsumables = updatedWorld.Consumables.Additions(currentWorld.Consumables).ToList(),
                RemovedConsumables = updatedWorld.Consumables.Removals(currentWorld.Consumables).ToList(),
                AgentUpdates = agentUpdates.ToList(),
                AddedAgents = agentAdditions.ToList(),
                RemovedAgents = agentRemovals.ToList(),
            };
        }

        private TerrainTile GetVegetationUpdate(TerrainTile tile, IEnumerable<TerrainTile> surroundingTiles)
        {
            var random = new Random();
            var randomValue = random.NextDouble();

            var vegitationChange = randomValue switch
            {
                var value when value > 0.9999 => 0.01f,
                var value when value > 0.9 && surroundingTiles.Any(t => t.VegetationLevel > 0) => 0.01f,
                _ when tile.VegetationLevel > 0 => 0.01f,
                _ => 0
            };

            return new TerrainTile
            {
                TerrainTileId = tile.TerrainTileId,
                VegetationLevel = tile.VegetationLevel + vegitationChange,
                TerrainType = tile.TerrainType,
                Height = tile.Height,
                XCoord = tile.XCoord,
                YCoord = tile.YCoord,
            };
        }
    }
}
