using LandSim.Areas.Map.Enums;
using LandSim.Shared;

namespace LandSim.Areas.Map.Models
{
    public record TerrainTile : BaseRecord, ILocation
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

        public bool IsWalkable() => TerrainType is not TerrainType.Water && TerrainType is not TerrainType.Void;
    }
}
