namespace LandSim.Areas.Map.Models
{
    public class Color
    {
        public float Red { get; set; }

        public float Green { get; set; }

        public float Blue { get; set; }

        public Color(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color(float greyScaleValue)
        {
            Red = greyScaleValue;
            Green = greyScaleValue;
            Blue = greyScaleValue;
        }

        public string GetCssColor()
        {
            return $"rgb({Red}, {Green}, {Blue})";
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            return new Color(Lerp(a.Red, b.Red, t), Lerp(a.Green, b.Green, t), Lerp(a.Blue, b.Blue, t));
            
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
