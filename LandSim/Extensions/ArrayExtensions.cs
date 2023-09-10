namespace LandSim.Extensions
{
    public static class ArrayExtensions
    {
        public static T? Left<T>(this T[,] array, int x, int y)
        {
            if (!array.IsInArray(x - 1, y))
            {
                return default;
            }

            return array[x - 1, y];
        }

        public static T? Right<T>(this T[,] array, int x, int y)
        {
            if (!array.IsInArray(x + 1, y))
            {
                return default;
            }

            return array[x + 1, y];
        }

        public static T? Up<T>(this T[,] array, int x, int y)
        {
            if (!array.IsInArray(x, y - 1))
            {
                return default;
            }

            return array[x, y - 1];
        }

        public static T? Down<T>(this T[,] array, int x, int y)
        {
            if (!array.IsInArray(x, y + 1))
            {
                return default;
            }

            return array[x, y + 1];
        }

        public static bool IsInArray<T>(this T[,] array, int x, int y)
        {
            return x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
        }

        public static IEnumerable<T> GetImmediateNeighbors<T>(this T[,] array, int x, int y)
        {
            var neighbors = new List<T?>
            {
                array.Left(x, y),
                array.Right(x, y),
                array.Up(x, y),
                array.Down(x, y)
            };

            return neighbors.Where(n => n != null).Select(n => n!);
        }

        public static IEnumerable<T> Flatten<T>(this T[,] map)
        {
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    yield return map[row, col];
                }
            }
        }


    }
}
