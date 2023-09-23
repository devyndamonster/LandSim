using LandSim.Areas.Agents.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using LandSim.Extensions;

namespace LandSim.Areas.Simulation.Services
{
    public class SimulationService
    {
        public WorldData GetUpdatedWorldData(WorldData currentWorldData, AgentAction[] actions)
        {
            var random = new Random();

            var agentActions = actions.ToDictionary(action => action.AgentId);

            //Agent actions
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
                        return agent.Value;
                    }

                    //TODO: Probably want to just combine world data grids at this point
                    var destination = action.ActionType switch
                    {
                        AgentActionType.MoveLeft => (
                            tile: currentWorldData.TerrainTiles.Left(agent.x, agent.y),
                            consumable: currentWorldData.Consumables.Left(agent.x, agent.y),
                            agent: currentWorldData.Agents.Left(agent.x, agent.y)),
                        AgentActionType.MoveRight => (
                            tile: currentWorldData.TerrainTiles.Right(agent.x, agent.y),
                            consumable: currentWorldData.Consumables.Right(agent.x, agent.y),
                            agent: currentWorldData.Agents.Right(agent.x, agent.y)),
                        AgentActionType.MoveUp => (
                            tile: currentWorldData.TerrainTiles.Up(agent.x, agent.y),
                            consumable: currentWorldData.Consumables.Up(agent.x, agent.y),
                            agent: currentWorldData.Agents.Up(agent.x, agent.y)),
                        AgentActionType.MoveDown => (
                            tile: currentWorldData.TerrainTiles.Down(agent.x, agent.y),
                            consumable: currentWorldData.Consumables.Down(agent.x, agent.y),
                            agent: currentWorldData.Agents.Down(agent.x, agent.y)),
                        _ => (
                            tile: currentWorldData.TerrainTiles[agent.x, agent.y],
                            consumable: currentWorldData.Consumables[agent.x, agent.y],
                            agent: currentWorldData.Agents[agent.x, agent.y]),
                    };

                    if (destination.tile is not null
                        && !agent.Value.IsAt(destination.tile)
                        && destination.tile.TerrainType != TerrainType.Water
                        && destination.consumable is null
                        && destination.agent is null)
                    {
                        return new Agent
                        {
                            AgentId = agent.Value.AgentId,
                            XCoord = destination.tile.XCoord,
                            YCoord = destination.tile.YCoord,
                            Hunger = agent.Value.Hunger,
                            Thirst = agent.Value.Thirst,
                        };
                    }

                    return agent.Value;
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

                return consumable switch
                {
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
            return new SimulationUpdates
            {
                UpdatedTiles = updatedWorld.TerrainTiles.Updates(currentWorld.TerrainTiles).ToList(),
                AddedConsumables = updatedWorld.Consumables.Additions(currentWorld.Consumables).ToList(),
                AgentUpdates = updatedWorld.Agents.Updates(currentWorld.Agents).ToList(),
                AddedAgents = updatedWorld.Agents.Additions(currentWorld.Agents).ToList(),
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
