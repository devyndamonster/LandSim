using System.Text.Json;
using LandSim.Areas.Map.Models;
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

            if(currentSettings != null)
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

        public void SaveTerrain(TerrainTile[,] terrain)
        {
            _mapContext.TerrainTiles.RemoveRange(_mapContext.TerrainTiles);

            for(var x = 0; x < terrain.GetLength(0); x++)
            {
                for(int y = 0; y < terrain.GetLength(1); y++)
                {
                    _mapContext.Add(terrain[x, y]);
                }
            }

            _mapContext.SaveChanges();
        }

        public TerrainTile[,] GetTerrain()
        {
            var terrain = _mapContext.TerrainTiles.ToList();

            var minX = terrain[0].XCoord;
            var maxX = terrain[0].XCoord;
            var minY = terrain[0].YCoord;
            var maxY = terrain[0].YCoord;

            foreach(var terrainTile in terrain)
            {
                if(terrainTile.XCoord < minX) minX = terrainTile.XCoord;
                if(terrainTile.YCoord < minY) minY = terrainTile.YCoord;
                if(terrainTile.XCoord > maxX) maxX = terrainTile.XCoord;
                if(terrainTile.YCoord > maxY) maxY = terrainTile.YCoord;
            }

            var sizeX = maxX - minX + 1;
            var sizeY = maxY - minY + 1;
            var terrainArray = new TerrainTile[sizeX, sizeY];

            foreach(var terrainTile in terrain)
            {
                terrainArray[terrainTile.XCoord - minX, terrainTile.YCoord - minY] = terrainTile;
            }

            return terrainArray;
        }

    }
}
