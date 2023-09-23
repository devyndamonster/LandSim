using System.Numerics;

namespace LandSim.Extensions
{
    public static class VectorExtensions
    {
        public static float Dot(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y;
        }
    }
}
