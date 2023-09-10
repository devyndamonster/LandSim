using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public class Bounds
    {
        public int MinX { get; set; }

        public int MaxX { get; set; }

        public int MinY { get; set; }

        public int MaxY { get; set; }

        public int SizeX => MaxX - MinX + 1;

        public int SizeY => MaxY - MinY + 1;

        public static Bounds FromLocations<T>(T[] locations) where T : ILocation
        {
            var bounds = new Bounds();

            foreach(var location in locations)
            {
                if (location.XCoord < bounds.MinX) bounds.MinX = location.XCoord;
                if (location.XCoord > bounds.MaxX) bounds.MaxX = location.XCoord;
                if (location.YCoord < bounds.MinY) bounds.MinY = location.YCoord;
                if (location.YCoord > bounds.MaxY) bounds.MaxY = location.YCoord;
            }

            return bounds;
        }
    }
}
