using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public class TerrainSelector
    {
        public int TerrainSelectorId { get; set; }

        public float MaxValue { get; set; } = 1f;

        public float MinValue { get; set; } = 0f;

        public TerrainType TerrainType { get; set; }

        public bool DoesApply(float value)
        {
            return value < MaxValue && value > MinValue;
        }
    }
}
