namespace LandSim.Areas.Map
{
    public class ColorSelector
    {
        public float MaxValue { get; set; } = 1f;

        public float MinValue { get; set; } = 0f;

        public Color Color { get; set; } = new Color(0f, 0f, 0f);

        public bool DoesApply(float value)
        {
            return value < MaxValue && value > MinValue;
        }
    }
}
