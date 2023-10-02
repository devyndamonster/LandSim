using LandSim.Areas.Agents.Models;
using LandSim.Areas.Map.Models;
using LandSim.Shared;

namespace LandSim.Extensions
{
    public static class LocationExtensions
    {
        public static TLocation?[,] MapLocationsToBoundedGrid<TLocation>(this IEnumerable<TLocation> locations, Bounds bounds) where TLocation : ILocation
        {
            var locationArray = new TLocation[bounds.SizeX, bounds.SizeY];

            foreach (var location in locations)
            {
                var indexX = location.XCoord - bounds.MinX;
                var indexY = location.YCoord - bounds.MinY;

                if (locationArray.IsInArray(indexX, indexY))
                {
                    locationArray[indexX, indexY] = location;
                }
            }

            return locationArray;
        }

        public static TLocation?[,] MapLocationsToBoundedGrid<TLocation>(this IEnumerable<TLocation> locations, Bounds bounds, Func<int, int, TLocation> getDefault) where TLocation : ILocation
        {
            var locationArray = locations.MapLocationsToBoundedGrid(bounds);

            for (int x = 0; x < locationArray.GetLength(0); x++)
            {
                for (int y = 0; y < locationArray.GetLength(1); y++)
                {
                    if (locationArray[x, y] is null)
                    {
                        var xCoord = x + bounds.MinX;
                        var yCoord = y + bounds.MinY;
                        locationArray[x, y] = getDefault(xCoord, yCoord);
                    }
                }
            }

            return locationArray;
        }

        public static IEnumerable<TLocation> MakeRelative<TLocation>(this IEnumerable<TLocation> locations, int xCoord, int yCoord) where TLocation : BaseRecord, ILocation 
        {
            foreach(var location in locations)
            {
                yield return location with
                {
                    XCoord = location.XCoord - xCoord,
                    YCoord = location.YCoord - yCoord
                };
            }
        }

        public static IEnumerable<TLocation> GetImmediateNeighbors<TLocation>(this TLocation?[,] locations, ILocation location) where TLocation : ILocation
        {
            for (int x = 0; x < locations.GetLength(0); x++)
            {
                for (int y = 0; y < locations.GetLength(1); y++)
                {
                    if (locations[x, y]?.IsAt(location) ?? false)
                    {
                        return locations.GetImmediateNeighbors(x, y);
                    }
                }
            }

            return Enumerable.Empty<TLocation>();
        }
    }
}
