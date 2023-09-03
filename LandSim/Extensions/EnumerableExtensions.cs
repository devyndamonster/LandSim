namespace LandSim.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var random = new Random();
            return collection.OrderBy(item => random.Next());
        }
    }
}
