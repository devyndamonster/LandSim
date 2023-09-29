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

        public static IEnumerable<T> GetElementsWithinRange<T>(this T?[,] array, int x, int y, int range)
        {
            for (int row = y - range; row <= y + range; row++)
            {
                for (int col = x - range; col <= x + range; col++)
                {
                    if (array.IsInArray(col, row) && array[col, row] is not null)
                    {
                        yield return array[col, row]!;
                    }
                }
            }
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

        public static IEnumerable<T> Updates<T>(this T?[,] array, T?[,] otherArray)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (array[x, y] is not null && otherArray[x, y] is not null && !array[x, y]!.Equals(otherArray[x, y]))
                    {
                        yield return array[x, y]!;
                    }
                }
            }
        }

        public static IEnumerable<T> Additions<T>(this T?[,] array, T?[,] otherArray)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (array[x, y] is not null && otherArray[x, y] is null)
                    {
                        yield return array[x, y]!;
                    }
                }
            }
        }

        public static IEnumerable<T> Removals<T>(this T?[,] array, T?[,] otherArray)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (array[x, y] is null && otherArray[x, y] is not null)
                    {
                        yield return otherArray[x, y]!;
                    }
                }
            }
        }
    }
}
