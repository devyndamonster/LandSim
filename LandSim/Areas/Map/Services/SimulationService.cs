using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using System;

namespace LandSim.Areas.Map.Services
{
    public class SimulationService
    {
        public SimulationUpdates GetSimulationUpdates(WorldData currentWorldData)
        {
            var random = new Random();

            var updatedTiles = currentWorldData.TerrainTiles
                .Select(tile =>
                {
                    var surroundingTiles = currentWorldData.TerrainTiles.GetImmediateNeighbors(tile.x, tile.y);

                    return tile switch
                    {
                        { Value.TerrainType: TerrainType.Soil } => (current: tile.Value, updated: GetVegetationUpdate(tile.Value, surroundingTiles)),
                        _ => (current: tile.Value, updated: tile.Value)
                    };
                })
                .Where(tile => tile.current?.HasChanged(tile.updated!) ?? false)
                .Select(tile => tile.updated!);

            var updatedConsumables = currentWorldData.TerrainTiles
                .Select(tile => tile)
                .Where(tile =>
                    currentWorldData.Consumables[tile.x, tile.y] is null
                    && tile.Value?.VegetationLevel > 0.95
                    && random.NextDouble() >= 0.99
                )
                .Select(tile => new Consumable
                {
                    XCoord = tile.x,
                    YCoord = tile.y,
                });

            return new SimulationUpdates
            {
                UpdatedTiles = updatedTiles.ToList(),
                AddedConsumables = updatedConsumables.ToList()
            };
        }

        private TerrainTile GetVegetationUpdate(TerrainTile tile, IEnumerable<TerrainTile> surroundingTiles)
        {
            var random = new Random();
            var randomValue = random.NextDouble();

            var vegitationChange = randomValue switch
            {
                var value when value > 0.999 => 0.01f,
                var value when value > 0.9 && surroundingTiles.Any(t => t.VegetationLevel > 0) => 0.01f,
                _ when tile.VegetationLevel > 0 => 0.01f,
                _ => 0
            };

            return new TerrainTile
            {
                VegetationLevel = tile.VegetationLevel + vegitationChange,
                XCoord = tile.XCoord,
                YCoord = tile.YCoord,
            };
        }
    }
}
