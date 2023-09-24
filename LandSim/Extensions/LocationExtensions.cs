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
    }
}
