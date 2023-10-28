using System.Text.Json;
using Dapper;
using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Generation.Models;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Database
{
    public class MapRepository
    {
        private readonly DatabaseConnection _connection;

        public MapRepository(DatabaseConnection connection)
        {
            _connection = connection;
        }

        public async Task ReplaceTerrain(TerrainTile?[,] terrain)
        {
            using var connection = _connection.GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var sql = "DELETE FROM TerrainTiles";
            await connection.ExecuteAsync(sql);

            sql = "DELETE FROM Consumables";
            await connection.ExecuteAsync(sql);

            sql = "DELETE FROM Agents";
            await connection.ExecuteAsync(sql);

            sql =
            """
                INSERT INTO TerrainTiles (TerrainType, Height, VegetationLevel, XCoord, YCoord)
                VALUES (@TerrainType, @Height, @VegetationLevel, @XCoord, @YCoord)
            """;

            foreach (var tile in terrain)
            {
                if (tile != null)
                {
                    await connection.ExecuteAsync(sql, tile);
                }
            }

            transaction.Commit();
        }
        
        public async Task SaveSimulationUpdates(SimulationUpdates updates)
        {
            using var connection = _connection.GetConnection();
            connection.Open();
            
            using var transaction = connection.BeginTransaction();

            var sql = 
            """
                UPDATE TerrainTiles
                SET TerrainType = @TerrainType, Height = @Height, VegetationLevel = @VegetationLevel
                WHERE TerrainTileId = @TerrainTileId
            """;
            
            foreach (var tile in updates.UpdatedTiles)
            {
                await connection.ExecuteAsync(sql, tile);
            }

            sql =
            """
                INSERT INTO Consumables (XCoord, YCoord)
                VALUES (@XCoord, @YCoord)
            """;

            foreach (var consumable in updates.AddedConsumables)
            {
                await connection.ExecuteAsync(sql, consumable);
            }

            sql =
            """
                DELETE FROM Consumables
                WHERE ConsumableId = @ConsumableId
            """;

            foreach (var consumable in updates.RemovedConsumables)
            {
                await connection.ExecuteAsync(sql, consumable);
            }

            sql =
            """
                UPDATE Agents
                SET 
                    XCoord = @XCoord, 
                    YCoord = @YCoord, 
                    Hunger = @Hunger, 
                    Thirst = @Thirst, 
                    ShortTermMemory = @ShortTermMemory, 
                    ReproductionCooldown = @ReproductionCooldown
                WHERE AgentId = @AgentId
            """;

            foreach (var agent in updates.AgentUpdates)
            {
                await connection.ExecuteAsync(sql, agent);
            }

            sql =
            """
                INSERT INTO Agents (XCoord, YCoord, AgentOwnerId, Hunger, Thirst, ReproductionCooldown)
                VALUES (@XCoord, @YCoord, @AgentOwnerId, @Hunger, @Thirst, @ReproductionCooldown)
            """;

            foreach (var agent in updates.AddedAgents)
            {
                await connection.ExecuteAsync(sql, agent);
            }

            sql =
            """
                DELETE FROM Agents
                WHERE AgentId = @AgentId
            """;

            foreach (var agent in updates.RemovedAgents)
            {
                await connection.ExecuteAsync(sql, agent);
            }

            transaction.Commit();
        }
        
        public async Task<WorldData> GetWorldData()
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                SELECT 
                    TerrainTileId,
                    TerrainType,
                    Height,
                    VegetationLevel,
                    XCoord,
                    YCoord
                FROM TerrainTiles
            """;

            var terrainTiles = await connection.QueryAsync<TerrainTile>(sql);

            sql =
            """
                SELECT
                    ConsumableId,
                    XCoord,
                    YCoord
                FROM Consumables
            """;

            var consumables = await connection.QueryAsync<Consumable>(sql);

            sql =
            """
                SELECT
                    AgentId,
                    XCoord,
                    YCoord,
                    AgentOwnerId,
                    Hunger,
                    Thirst,
                    ShortTermMemory,
                    ReproductionCooldown
                FROM Agents
            """;

            var agents = await connection.QueryAsync<Agent>(sql);

            return new WorldData(terrainTiles.ToArray(), consumables.ToArray(), agents.ToArray());
        }

        public async Task SetAgentAction(AgentAction action)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                INSERT INTO AgentActions (AgentId, ActionType, UpdatedShortTermMemory)
                VALUES (@AgentId, @ActionType, @UpdatedShortTermMemory)
                ON CONFLICT (AgentId) DO UPDATE SET ActionType = @ActionType
            """;

            await connection.ExecuteAsync(sql, action);
        }

        public async Task<List<AgentAction>> PopAgentActions()
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                DELETE FROM AgentActions RETURNING AgentId, ActionType, UpdatedShortTermMemory
            """;

            var actions = await connection.QueryAsync<AgentAction>(sql);

            return actions.ToList();
        }

        public async Task<List<AgentOwner>> GetAgentOwners()
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                SELECT
                    AgentOwnerId,
                    Key,
                    PostbackUrl
                FROM AgentOwner
            """;

            return (await connection.QueryAsync<AgentOwner>(sql)).ToList();
        }

        public async Task<AgentOwner> GetAgentOwner(int agentOwnerId)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                SELECT
                    AgentOwnerId,
                    Key,
                    PostbackUrl
                FROM AgentOwner
                WHERE AgentOwnerId = @AgentOwnerId
            """;

            return await connection.QuerySingleAsync<AgentOwner>(sql, new { AgentOwnerId = agentOwnerId });
        }

        public async Task<int> InsertAgentOwner(AgentOwner owner)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                INSERT INTO AgentOwner (Key, PostbackUrl)
                VALUES (@Key, @PostbackUrl)
                RETURNING AgentOwnerId
            """;

            return await connection.QuerySingleAsync<int>(sql, owner);
        }

        public async Task DeleteAgentOwner(AgentOwner owner)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                DELETE FROM AgentOwner
                WHERE AgentOwnerId = @AgentOwnerId
            """;

            await connection.ExecuteAsync(sql, owner);
        }

        public async Task UpdateAgentOwner(AgentOwner owner)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                UPDATE AgentOwner
                SET Key = @Key, PostbackUrl = @PostbackUrl
                WHERE AgentOwnerId = @AgentOwnerId
            """;

            await connection.ExecuteAsync(sql, owner);
        }

        public async Task<IEnumerable<SimulationConfig>> GetConfigs()
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                SELECT
                    *
                FROM SimulationConfigs
            """;

            return await connection.QueryAsync<SimulationConfig>(sql);
        }

        public async Task UpdateConfig(SimulationConfig config)
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                UPDATE SimulationConfigs
                SET ConsumableSpawnChance = @ConsumableSpawnChance,
                    ConsumableHungerIncrease = @ConsumableHungerIncrease,
                    ConsumableVegitationSpawnThreshold = @ConsumableVegitationSpawnThreshold,
                    BaseHungerCost = @BaseHungerCost,
                    MovementHungerCost = @MovementHungerCost,
                    ClimbHungerCost = @ClimbHungerCost,
                    AgentSpawnChange = @AgentSpawnChange,
                    BaseThirstCost = @BaseThirstCost,
                    DrinkThirstIncrease = @DrinkThirstIncrease,
                    VegitationMovementHungerCost = @VegitationMovementHungerCost,
                    VegitationSpawnChance = @VegitationSpawnChance,
                    VegitationSpreadChance = @VegitationSpreadChance,
                    VegitationGrowthRate = @VegitationGrowthRate,
                    VegitationDecreaseFromMovement = @VegitationDecreaseFromMovement,
                    ReproductionCooldownRate = @ReproductionCooldownRate,
                    IsPaused = @IsPaused,
                    SimulationRate = @SimulationRate
                WHERE SimulationConfigId = @SimulationConfigId
            """;

            await connection.ExecuteAsync(sql, config);
        }

        public async Task<int> InsertConfig(SimulationConfig config)
        {
            var connection = _connection.GetConnection();

            var sql =
            """
                INSERT INTO SimulationConfigs (
                    ConsumableSpawnChance,
                    ConsumableHungerIncrease,
                    ConsumableVegitationSpawnThreshold,
                    BaseHungerCost,
                    MovementHungerCost,
                    ClimbHungerCost,
                    AgentSpawnChange,
                    BaseThirstCost,
                    DrinkThirstIncrease,
                    VegitationMovementHungerCost,
                    VegitationSpawnChance,
                    VegitationSpreadChance,
                    VegitationGrowthRate,
                    VegitationDecreaseFromMovement,
                    ReproductionCooldownRate,
                    IsPaused,
                    SimulationRate
                )
                VALUES (
                    @ConsumableSpawnChance,
                    @ConsumableHungerIncrease,
                    @ConsumableVegitationSpawnThreshold,
                    @BaseHungerCost,
                    @MovementHungerCost,
                    @ClimbHungerCost,
                    @AgentSpawnChange,
                    @BaseThirstCost,
                    @DrinkThirstIncrease,
                    @VegitationMovementHungerCost,
                    @VegitationSpawnChance,
                    @VegitationSpreadChance,
                    @VegitationGrowthRate,
                    @VegitationDecreaseFromMovement,
                    @ReproductionCooldownRate,
                    @IsPaused,
                    @SimulationRate
                )
                RETURNING SimulationConfigId
            """;

            return await connection.QuerySingleAsync<int>(sql, config);
        }
    }
}
