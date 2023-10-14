using AntDesign;
using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Extensions;
using System;

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
                    var consumable = currentWorldData.Consumables[agent.x, agent.y];
                    var neighboringTiles = currentWorldData.TerrainTiles.GetImmediateNeighbors(agent.x, agent.y);

                    Func<Agent> getUpdatedAgent = actionType switch
                    {
                        AgentActionType.Eat when consumable is not null => () =>
                        {
                            return agent.Value with
                            {
                                Hunger = agent.Value.Hunger + config.ConsumableHungerIncrease,
                                Thirst = agent.Value.Thirst - config.BaseThirstCost
                            };
                        },
                        AgentActionType.Drink when neighboringTiles.Any(tile => tile.TerrainType == TerrainType.Water) => () =>
                        {
                            return agent.Value with
                            {
                                Thirst = agent.Value.Thirst + config.DrinkThirstIncrease,
                                Hunger = agent.Value.Hunger - config.BaseHungerCost
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
                                Thirst = agent.Value.Thirst - config.BaseThirstCost
                            };
                        },
                        _ => () => agent.Value with
                        {
                            Hunger = agent.Value.Hunger - config.BaseHungerCost,
                            Thirst = agent.Value.Thirst - config.BaseThirstCost
                        }
                    };

                    var updatedAgent = getUpdatedAgent();

                    return updatedAgent.Hunger > 0 && updatedAgent.Thirst > 0 ? updatedAgent : null;
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

            //TODO: We could have context object and chain calls for simulation updates

            updatedAgentGrid = GetWithNewAgents(currentWorldData, updatedAgentGrid, agents, agentActions, agentOwners, config);

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

        private Agent?[,] GetWithNewAgents(
            WorldData worldData, 
            Agent?[,] agentGrid, 
            Dictionary<int, Agent> agents,
            Dictionary<int, AgentAction> agentActions,
            IEnumerable<AgentOwner> agentOwners,
            SimulationConfig config)
        {
            var random = new Random();

            var reproductionRequests = agentGrid
                .Where(agent => agent.Value is not null)
                .Where(agent =>
                {
                    agentActions.TryGetValue(agent.Value!.AgentId, out var action);
                    return action?.ActionType == AgentActionType.Reproduce;
                })
                .ToDictionary(
                    agent => agent.Value!.AgentId,
                    agent =>
                    {
                        var otherAgent = agentGrid.GetImmediateNeighbors(agent.x, agent.y)
                            .Where(other => other.AgentOwnerId == agent.Value!.AgentOwnerId)
                            .FirstOrDefault();

                        return otherAgent?.AgentId;
                    });

            var successfulParents = reproductionRequests
                .Where(request =>
                {
                    var self = request.Key;
                    var target = request.Value;

                    if (target is null)
                    {
                        return false;
                    }

                    if (reproductionRequests.TryGetValue(target.Value, out var otherTarget))
                    {
                        return otherTarget == self;
                    }

                    return false;
                })
                .Select(pair => Math.Min(pair.Key, pair.Value!.Value))
                .Distinct();

            var newChildAgents = successfulParents
                .Select(parentId =>
                {
                    var parent = agents[parentId];
                    var agentGridPostition = worldData.Bounds.GetIndexesFromLocation(parent);

                    var neighboringTiles = worldData.TerrainTiles.GetImmediateNeighbors(agentGridPostition.x, agentGridPostition.y);
                    var neighboringAgents = agentGrid.GetImmediateNeighbors(agentGridPostition.x, agentGridPostition.y);

                    var spawnLocation = neighboringTiles
                        .Where(tile => tile.IsWalkable())
                        .Where(tile => neighboringAgents?.FirstOrDefault(tile.IsAt) is null)
                        .Shuffle()
                        .FirstOrDefault();

                    return spawnLocation is not null ? new Agent
                    {
                        XCoord = spawnLocation.XCoord,
                        YCoord = spawnLocation.YCoord,
                        AgentOwnerId = parent.AgentOwnerId,
                        Hunger = 1,
                        Thirst = 1,
                    } : null;
                })
                .OfType<Agent>();

            return agentGrid
                .Map(agent =>
                {
                    var tile = worldData.TerrainTiles[agent.x, agent.y];

                    if (agent.Value is null && tile is not null)
                    {
                        if (newChildAgents.FirstOrDefault(tile.IsAt) is Agent child)
                        {
                            return child;
                        }

                        else if (tile.TerrainType == TerrainType.Sand && random.NextDouble() <= config.AgentSpawnChange)
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
        }
    }
}
