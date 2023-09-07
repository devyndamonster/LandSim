using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public class TerrainTile
    {
        public int TerrainTileId { get; set; }

        public int XCoord { get; set; }

        public int YCoord { get; set; }

        public TerrainType TerrainType { get; set; }

        public float Height { get; set; }
    }
}
