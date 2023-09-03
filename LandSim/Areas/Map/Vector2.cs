namespace LandSim.Areas.Map
{
    public class Vector2
    {
        public float X { get; set; }

        public float Y { get; set; }

        public Vector2 (float x, float y)
        {
            X = x; 
            Y = y;
        }

        public float Dot(Vector2 v)
        {
            return X * v.X + Y * v.Y;
        }
    }
}
