namespace LandSim.Areas.Map
{
    public class GenerationSettings
    {
        public string Seed { get; set; } = string.Empty;

        public int Width { get; set; } = 50;

        public int Height { get; set; } = 50;

        public float Frequency { get; set; } = 0.055f;

        public List<ColorSelector> ColorSelectors { get; set; } = new List<ColorSelector>();
    }
}
