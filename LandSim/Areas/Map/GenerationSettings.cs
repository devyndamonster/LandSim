namespace LandSim.Areas.Map
{
    public class GenerationSettings
    {
        public string Seed { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public float Frequency { get; set; } = 0.055f;
    }
}
