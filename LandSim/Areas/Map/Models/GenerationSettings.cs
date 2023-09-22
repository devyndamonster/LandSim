namespace LandSim.Areas.Map.Models
{
    public class GenerationSettings
    {
        public int GenerationSettingsId { get; set; }

        public string Seed { get; set; } = string.Empty;

        public int Width { get; set; } = 50;

        public int Height { get; set; } = 50;

        public float Frequency { get; set; } = 0.055f;

        public List<TerrainSelector> TerrainSelectors { get; set; } = new List<TerrainSelector>();
    }
}
