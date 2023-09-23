using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;

namespace LandSim.Areas.Map.Services
{
    public class TerrainService
    {
        public Color GetColorForTerrain(TerrainTile tile)
        {
            var baseColor = GetColorForTerrain(tile.TerrainType);
            var vegitationColor = new Color(0, 200, 0);
            return Color.Lerp(baseColor, vegitationColor, tile.VegetationLevel);
        }
        
        public Color GetColorForTerrain(TerrainType terrainType)
        {
            return terrainType switch
            {
                TerrainType.Water => new Color(0, 0, 200),
                TerrainType.Sand => new Color(200, 200, 0),
                TerrainType.Soil => new Color(100, 100, 0),
                TerrainType.Rock => new Color(100),
                _ => new Color(0)
            };
        }
    }
}
