using LandSim.Areas.Generation.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using System.Numerics;

namespace LandSim.Areas.Generation.Services
{
    public class TerrainGenerationService
    {
        public TerrainTile[,] GenerateTerrain(GenerationSettings settings)
        {
            var terrain = new TerrainTile[settings.Height, settings.Width];
            var permutations = GetPermutations(settings);

            for (var x = 0; x < settings.Height; x++)
            {
                for (var y = 0; y < settings.Width; y++)
                {
                    var perlinNoiseValue = PerlinNoise(settings.Frequency * x, settings.Frequency * y, permutations);

                    var terrainType = settings.TerrainSelectors.FirstOrDefault(selector =>
                        selector.DoesApply(perlinNoiseValue))?.TerrainType ?? TerrainType.Water;

                    terrain[x, y] = new TerrainTile
                    {
                        XCoord = x,
                        YCoord = y,
                        TerrainType = terrainType,
                        Height = perlinNoiseValue
                    };
                }
            }

            return terrain;
        }

        private List<int> GetPermutations(GenerationSettings settings)
        {
            var seed = settings.Seed.GetDeterministicHashCode();
            var permutations = Enumerable.Range(0, 255).Shuffle(new Random(seed)).ToList();
            permutations.AddRange(permutations);
            return permutations;
        }

        private float PerlinNoise(float x, float y, List<int> permutations)
        {
            var xCoord = (int)MathF.Floor(x) % 256;
            var yCoord = (int)MathF.Floor(y) % 256;
            var xFloat = x - MathF.Floor(x);
            var yFloat = y - MathF.Floor(y);

            //Get vectors pointing to the value from the corners of the tile
            var topRight = new Vector2(xFloat - 1.0f, yFloat - 1.0f);
            var topLeft = new Vector2(xFloat, yFloat - 1.0f);
            var bottomRight = new Vector2(xFloat - 1.0f, yFloat);
            var bottomLeft = new Vector2(xFloat, yFloat);

            var valueTopRight = permutations[permutations[xCoord + 1] + yCoord + 1];
            var valueTopLeft = permutations[permutations[xCoord] + yCoord + 1];
            var valueBottomRight = permutations[permutations[xCoord + 1] + yCoord];
            var valueBottomLeft = permutations[permutations[xCoord] + yCoord];

            var dotTopRight = topRight.Dot(GetConstantCornerVector(valueTopRight));
            var dotTopLeft = topLeft.Dot(GetConstantCornerVector(valueTopLeft));
            var dotBottomRight = bottomRight.Dot(GetConstantCornerVector(valueBottomRight));
            var dotBottomLeft = bottomLeft.Dot(GetConstantCornerVector(valueBottomLeft));

            var u = Fade(xFloat);
            var v = Fade(yFloat);

            var perlineNoise = Lerp(u, Lerp(v, dotBottomLeft, dotTopLeft), Lerp(v, dotBottomRight, dotTopRight));

            return (perlineNoise + 1) / 2;
        }

        private Vector2 GetConstantCornerVector(int randomValue)
        {
            return (randomValue % 4) switch
            {
                0 => new Vector2(1, 1),
                1 => new Vector2(-1, 1),
                2 => new Vector2(-1, -1),
                _ => new Vector2(1, -1)
            };
        }

        private float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        private float Fade(float t)
        {
            return ((6 * t - 15) * t + 10) * t * t * t;
        }
    }
}
