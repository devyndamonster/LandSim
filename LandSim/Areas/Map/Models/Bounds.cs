using LandSim.Areas.Map.Enums;

namespace LandSim.Areas.Map.Models
{
    public class Bounds
    {
        public int MinX { get; init; }

        public int MaxX { get; init; }

        public int MinY { get; init; }

        public int MaxY { get; init; }

        public int SizeX => MaxX - MinX + 1;

        public int SizeY => MaxY - MinY + 1;

        public static Bounds FromLocations<T>(T[] locations) where T : ILocation
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            foreach(var location in locations)
            {
                if (location.XCoord < minX) minX = location.XCoord;
                if (location.XCoord > maxX) maxX = location.XCoord;
                if (location.YCoord < minY) minY = location.YCoord;
                if (location.YCoord > maxY) maxY = location.YCoord;
            }

            return new Bounds
            {
                MaxX = maxX,
                MinX = minX,
                MaxY = maxY,
                MinY = minY
            };
        }

        public (int x, int y) GetIndexesFromLocation(ILocation location)
        {
            return (location.XCoord - MinX, location.YCoord - MinY);
        }
    }
}
