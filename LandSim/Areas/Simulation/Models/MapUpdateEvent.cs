using LandSim.Areas.Agents.Models;
using LandSim.Areas.Map.Models;

namespace LandSim.Areas.Simulation.Models
{
    public class MapUpdateEvent
    {
        public TerrainTile?[,] TerrainTiles { get; set; } = new TerrainTile?[0, 0];

        public Consumable?[,] Consumables { get; set; } = new Consumable?[0, 0];

        public Agent?[,] Agents { get; set; } = new Agent?[0, 0];
    }
}
