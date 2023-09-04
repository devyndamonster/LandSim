namespace LandSim.Areas.Map
{
    public class Color
    {
        public float Red {  get; set; }

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
    }
}
