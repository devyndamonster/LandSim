using System.Text.Json;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map
{
    public class MapGenerationRepository
    {
        private readonly MapContext _mapContext;

        public MapGenerationRepository(MapContext mapContext)
        {
            _mapContext = mapContext;
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

            foreach (var tile in terrain)
            {
                if (tile != null)
                {
                    _mapContext.TerrainTiles.Add(tile);
                }
            }

            _mapContext.SaveChanges();
        }

        public void SaveTerrain(TerrainTile?[,] terrain)
        {
            var updatedTerrain = terrain.Flatten();
            _mapContext.TerrainTiles.UpdateRange(updatedTerrain);
            _mapContext.SaveChanges();
        }

        public void AddConsumables(IEnumerable<Consumable> consumables)
        {
            _mapContext.Consumables.AddRange(consumables);
            _mapContext.SaveChanges();
        }

        public void RemoveConsumables(IEnumerable<Consumable> consumables)
        {
            _mapContext.Consumables.RemoveRange(consumables);
            _mapContext.SaveChanges();
        }

        public WorldData GetWorldData()
        {
            return new WorldData(_mapContext.TerrainTiles.ToArray(), _mapContext.Consumables.ToArray());
        }

    }
}
