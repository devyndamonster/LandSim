using LandSim.Areas.Map.Models;

namespace LandSim.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var random = new Random();
            return collection.Shuffle(random);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection, Random random)
        {
            return collection.OrderBy(item => random.Next());
        }

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
    }
}
