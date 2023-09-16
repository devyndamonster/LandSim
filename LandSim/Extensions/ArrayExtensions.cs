using LandSim.Areas.Map.Models;

namespace LandSim.Extensions
{
    public static class ArrayExtensions
    {
        public static T? Left<T>(this T?[,] array, int x, int y)
        {
            if (!array.IsInArray(x - 1, y))
            {
                return default;
            }

            return array[x - 1, y];
        }

        public static T? Right<T>(this T?[,] array, int x, int y)
        {
            if (!array.IsInArray(x + 1, y))
            {
                return default;
            }

            return array[x + 1, y];
        }

        public static T? Up<T>(this T?[,] array, int x, int y)
        {
            if (!array.IsInArray(x, y - 1))
            {
                return default;
            }

            return array[x, y - 1];
        }

        public static T? Down<T>(this T?[,] array, int x, int y)
        {
            if (!array.IsInArray(x, y + 1))
            {
                return default;
            }

            return array[x, y + 1];
        }

        public static bool IsInArray<T>(this T?[,] array, int x, int y)
        {
            return x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
        }

        public static IEnumerable<T> GetImmediateNeighbors<T>(this T?[,] array, int x, int y)
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

        public static IEnumerable<T> Flatten<T>(this T?[,] map)
        {
            var flattened = new List<T>();

            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    if (map[row, col] != null)
                    {
                        flattened.Add(map[row, col]!);
                    }
                }
            }

            return flattened;
        }
        
        public static IEnumerable<TResult> Select<T, TResult>(this T?[,] map, Func<(int x, int y, T? Value), TResult> func)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    yield return func((x, y, map[x, y]));
                }
            }
        }

        public static T?[,] Map<T>(this T?[,] array, Func<(int x, int y, T? Value), T?> func)
        {
            var newArray = new T?[array.GetLength(0), array.GetLength(1)];

            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    newArray[x, y] = func((x, y, array[x, y]));
                }
            }

            return newArray;
        }
    }
}
