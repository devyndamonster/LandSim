using System.Text.Json;
using Dapper;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map
{
    public class MapGenerationRepository
    {
        private readonly MapContext _mapContext;
        private readonly DatabaseConnection _connection;

        public MapGenerationRepository(MapContext mapContext, DatabaseConnection connection)
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
                INSERT INTO Agents (XCoord, YCoord, Hunger, Thirst)
                VALUES (@XCoord, @YCoord, @Hunger, @Thirst)
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
                    Hunger,
                    Thirst
                FROM Agents
            """;

            var agents = await connection.QueryAsync<Agent>(sql);

            return new WorldData(terrainTiles.ToArray(), consumables.ToArray(), agents.ToArray());
        }
    }
}
