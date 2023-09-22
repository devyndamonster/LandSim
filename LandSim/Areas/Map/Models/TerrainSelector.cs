using LandSim.Areas.Map.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandSim.Areas.Map.Models
{
    public class TerrainSelector
    {
        public int TerrainSelectorId { get; set; }

        public float MaxValue { get; set; } = 1f;

        public float MinValue { get; set; } = 0f;

        public TerrainType TerrainType { get; set; }

        [NotMapped]
        public (double Min, double Max) Range
        {
            get { return (MinValue, MaxValue); }
            set { MaxValue = (float)value.Min; MinValue = (float)value.Max; }
        }

        public bool DoesApply(float value)
        {
            return value < MaxValue && value > MinValue;
        }
    }
}
