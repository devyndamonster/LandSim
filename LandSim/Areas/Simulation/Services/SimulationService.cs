using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Extensions;

namespace LandSim.Areas.Simulation.Services
{
    public class SimulationService
    {
        public WorldData GetUpdatedWorldData(WorldData currentWorldData, SimulationConfig config, IEnumerable<AgentOwner> agentOwners, IEnumerable<AgentAction> actions)
        {
            var random = new Random();

            var agentActions = actions.ToDictionary(action => action.AgentId);

            var agents = currentWorldData.Agents
                .Where(agent => agent.Value is not null)
                .ToDictionary(agent => agent.Value!.AgentId, agent => agent.Value!);

            //Choose agents destination based on their action
            var agentDestinations = currentWorldData.Agents
                .Where(agent => agent.Value is not null)
                .ToDictionary(
                    agent => agent.Value!.AgentId,
                    agent =>
                    {
                        agentActions.TryGetValue(agent.Value!.AgentId, out var action);
                        var actionType = action?.ActionType ?? AgentActionType.None;

                        return actionType switch
                        {
                            AgentActionType.MoveLeft => currentWorldData.TerrainTiles.Left(agent.x, agent.y),
                            AgentActionType.MoveRight => currentWorldData.TerrainTiles.Right(agent.x, agent.y),
                            AgentActionType.MoveUp => currentWorldData.TerrainTiles.Up(agent.x, agent.y),
                            AgentActionType.MoveDown => currentWorldData.TerrainTiles.Down(agent.x, agent.y),
                            _ => currentWorldData.TerrainTiles[agent.x, agent.y],
                        };
                    });

            //For agents trying to move to the same destination, choose one of them to go there and reset the rest
            agentDestinations = agentDestinations
                .ToDictionary(
                    pair => pair.Key,
                    pair =>
                    {
                        var agentId = pair.Key;
                        var destination = pair.Value;
                        var agent = agents[agentId];

                        if (destination is null)
                        {
                            return destination;
                        }

                        if (agent.IsAt(destination))
                        {
                            return destination;
                        }

                        var otherAtDestination = agents.Values.FirstOrDefault(other => other.AgentId != agentId && other.IsAt(destination));

                        if(otherAtDestination is not null)
                        {
                            return destination with
                            {
                                XCoord = agent.XCoord,
                                YCoord = agent.YCoord,
                            };
                        }

                        var othersMovingToDestination = agentDestinations
                            .Where(dest => 
                                dest.Value is not null 
                                && dest.Value.XCoord == destination.XCoord 
                                && dest.Value.YCoord == destination.YCoord 
                                && dest.Key != agentId)
                            .Select(dest => agents[dest.Key]);

                        //The agent with the minimum id gets to go to the destination
                        if(othersMovingToDestination.Any())
                        {
                            var minId = othersMovingToDestination.Min(other => other.AgentId);

                            if (agentId > minId || othersMovingToDestination.Any(other => other.IsAt(destination)))
                            {
                                return destination with
                                {
                                    XCoord = agent.XCoord,
                                    YCoord = agent.YCoord,
                                };
                            }
                        }

                        return destination;
                    });

            //Now update agents based on their actions
            var updatedAgentGrid = currentWorldData.Agents
                .Where(agent => agent.Value is not null)
                .Select(agent =>
                {
                    agentActions.TryGetValue(agent.Value!.AgentId, out var action);
                    var actionType = action?.ActionType ?? AgentActionType.None;
                    var destination = agentDestinations[agent.Value.AgentId];

                    Func<Agent> getUpdatedAgent = actionType switch
                    {
                        AgentActionType.Eat => () =>
                        {
                            var consumable = currentWorldData.Consumables[agent.x, agent.y];
                            var hungerDelta = consumable is not null ? config.ConsumableHungerIncrease : -config.BaseHungerCost;

                            return agent.Value with
                            {
                                Hunger = agent.Value.Hunger + hungerDelta,
                            };
                        },
                        _ when actionType.IsMove() && (destination?.IsWalkable() ?? false) => () =>
                        {
                            var currentTile = currentWorldData.TerrainTiles[agent.x, agent.y]!;
                            var heightIncrease = MathF.Max(0, destination.Height - currentTile.Height);

                            var hungerDecrease = 
                                config.BaseHungerCost 
                                + config.MovementHungerCost 
                                + (heightIncrease * config.ClimbHungerCost) 
                                + (destination.VegetationLevel * config.VegitationMovementHungerCost);

                            return agent.Value with
                            {
                                XCoord = destination.XCoord,
                                YCoord = destination.YCoord,
                                Hunger = agent.Value.Hunger - hungerDecrease,
                            };
                        },
                        _ => () => agent.Value with
                        {
                            Hunger = agent.Value.Hunger - config.BaseHungerCost
                        }
                    };

                    var updatedAgent = getUpdatedAgent();

                    return updatedAgent.Hunger > 0 ? updatedAgent : null;
                })
                .OfType<Agent>()
                .Select(agent =>
                {
                    agentActions.TryGetValue(agent.AgentId, out var action);
                    return agent with
                    {
                        ShortTermMemory = action?.UpdatedShortTermMemory ?? agent.ShortTermMemory,
                    };
                })
                .MapLocationsToBoundedGrid(currentWorldData.Bounds);
                
            //New Agents
            updatedAgentGrid = updatedAgentGrid.Map(agent =>
            {
                if (agent.Value is null)
                {
                    var tile = currentWorldData.TerrainTiles[agent.x, agent.y];

                    if (tile?.TerrainType == TerrainType.Sand && random.NextDouble() <= config.AgentSpawnChange)
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
                var random = new Random();

                Func<TerrainTile?> getUpdatedTile = tile switch
                {
                    { Value.TerrainType: TerrainType.Soil } => () => {
                        var agentOnTile = updatedAgentGrid[tile.x, tile.y];
                        var agentAction = agentActions.GetValueOrDefault(agentOnTile?.AgentId ?? -1)?.ActionType ?? AgentActionType.None;
                        var randomValue = random.NextDouble();

                        var vegitationChange = randomValue switch
                        {
                            _ when agentAction.IsMove() => -config.VegitationDecreaseFromMovement,
                            var rand when rand <= config.VegitationSpawnChance => config.VegitationGrowthRate,
                            var rand when rand <= config.VegitationSpreadChance && surroundingTiles.Any(t => t.VegetationLevel > 0) => config.VegitationGrowthRate,
                            _ when tile.Value.VegetationLevel > 0 => config.VegitationGrowthRate,
                            _ => 0
                        };

                        return tile.Value with { VegetationLevel = tile.Value.VegetationLevel + vegitationChange };
                    },
                    _ => () => tile.Value
                };

                var updatedTile = getUpdatedTile();

                return updatedTile;
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
                        && terrainTile?.VegetationLevel > config.ConsumableVegitationSpawnThreshold
                        && random.NextDouble() <= config.ConsumableSpawnChance =>
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
                .OfType<Agent>();

            var updatedAgents = updatedWorld.Agents
                .Select(agent => agent.Value)
                .OfType<Agent>();

            var agentUpdates = updatedAgents
                .Where(agent =>
                {
                    if (currentAgents.FirstOrDefault(current => current.AgentId == agent.AgentId) is Agent currentAgent)
                    {
                        return !agent.Equals(currentAgent);
                    }

                    return false;
                });

            var agentAdditions = updatedAgents
                .Where(updated => !currentAgents.Any(current => current.AgentId == updated.AgentId));

            var agentRemovals = currentAgents
                .Where(current => !updatedAgents.Any(updated => updated.AgentId == current.AgentId));

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
    }
}
