using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public record TerrainTile : ILocation
    {
        public int TerrainTileId { get; init; }

        public int XCoord { get; init; }

        public int YCoord { get; init; }

        public TerrainType TerrainType { get; init; }

        public float Height { get; init; }

        private float _vegitiationLevel { get; set; }

        public float VegetationLevel
        { 
            get => _vegitiationLevel;
            init => _vegitiationLevel = Math.Clamp(value, 0, 1);
        }
    }
}
