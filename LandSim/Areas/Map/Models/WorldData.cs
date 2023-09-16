using LandSim.Extensions;

namespace LandSim.Areas.Map.Models
{
    public class WorldData
    {
        public TerrainTile?[,] TerrainTiles { get; set; }

        public Consumable?[,] Consumables { get; set; }

        public Agent?[,] Agents { get; set; }

        public Bounds Bounds { get; set; }
        

        public WorldData(TerrainTile[] terrainTiles, Consumable[] consumables, Agent[] agents)
        {
            Bounds = Bounds.FromLocations(terrainTiles);
            TerrainTiles = terrainTiles.MapLocationsToBoundedGrid(Bounds);
            Consumables = consumables.MapLocationsToBoundedGrid(Bounds);
            Agents = agents.MapLocationsToBoundedGrid(Bounds);
        }
    }
}
