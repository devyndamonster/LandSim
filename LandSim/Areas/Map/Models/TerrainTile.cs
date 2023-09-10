﻿using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public class TerrainTile
    {
        public int TerrainTileId { get; set; }

        public int XCoord { get; set; }

        public int YCoord { get; set; }

        public TerrainType TerrainType { get; set; }

        public float Height { get; set; }

        private float _vegitiationLevel { get; set; }

        public float VegetationLevel { 
            get => _vegitiationLevel;
            set => _vegitiationLevel = Math.Clamp(value, 0, 1);
        }

        public TerrainTile Clone()
        {
            return new TerrainTile
            {
                TerrainTileId = TerrainTileId,
                XCoord = XCoord,
                YCoord = YCoord,
                TerrainType = TerrainType,
                Height = Height,
                VegetationLevel = VegetationLevel
            };
        }
    }
}