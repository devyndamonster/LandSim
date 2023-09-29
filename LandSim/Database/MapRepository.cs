using System.Text.Json;
using Dapper;
using LandSim.Areas.Agents.Models;
using LandSim.Areas.Generation.Models;
using LandSim.Areas.Map.Models;
using LandSim.Areas.Simulation.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Database
{
    public class MapRepository
    {
        private readonly MapContext _mapContext;
        private readonly DatabaseConnection _connection;

        public MapRepository(MapContext mapContext, DatabaseConnection connection)
        {
            _mapContext = mapContext;
            _connection = connection;
        }

        public void SaveSettings(GenerationSettings settings)
        {
            var currentSettings = _mapContext.GenerationSettings.FirstOrDefault(s => s.GenerationSettingsId == settings.GenerationSettingsId);
            
            if (currentSettings != null)
            {
                _mapContext.Update(settings);
            }
            else
            {
                _mapContext.GenerationSettings.Add(settings);
            }

            _mapContext.SaveChanges();
        }

        public GenerationSettings GetSettings()
        {
            var settings = _mapContext.GenerationSettings
                .Include(setting => setting.TerrainSelectors)
                .ToList()
                .FirstOrDefault();

            return settings ?? new GenerationSettings();
        }

        public void ReplaceTerrain(TerrainTile?[,] terrain)
        {
            _mapContext.Database.ExecuteSqlRaw("DELETE FROM TerrainTiles");
            _mapContext.Database.ExecuteSqlRaw("DELETE FROM Consumables");
            _mapContext.Database.ExecuteSqlRaw("DELETE FROM Agents");

            foreach (var tile in terrain)
            {
                if (tile != null)
                {
                    _mapContext.TerrainTiles.Add(tile);
                }
            }

            _mapContext.SaveChanges();
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
                UPDATE Agents
                SET XCoord = @XCoord, YCoord = @YCoord, Hunger = @Hunger, Thirst = @Thirst
                WHERE AgentId = @AgentId
            """;

            foreach (var agent in updates.AgentUpdates)
            {
                await connection.ExecuteAsync(sql, agent);
            }

            sql =
            """
                INSERT INTO Agents (XCoord, YCoord, AgentOwnerId, Hunger, Thirst)
                VALUES (@XCoord, @YCoord, @AgentOwnerId, @Hunger, @Thirst)
            """;

            foreach (var agent in updates.AddedAgents)
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
                    Thirst
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
                INSERT INTO AgentActions (AgentId, ActionType)
                VALUES (@AgentId, @ActionType)
                ON CONFLICT (AgentId) DO UPDATE SET ActionType = @ActionType
            """;

            await connection.ExecuteAsync(sql, action);
        }

        public async Task<List<AgentAction>> PopAgentActions()
        {
            using var connection = _connection.GetConnection();

            var sql =
            """
                DELETE FROM AgentActions RETURNING AgentId, ActionType
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
    }
}
