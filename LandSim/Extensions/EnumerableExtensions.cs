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
    }
}
