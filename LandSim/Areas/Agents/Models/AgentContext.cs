using LandSim.Areas.Map.Models;

namespace LandSim.Areas.Agents.Models
{
    public class AgentContext
    {
        public Agent Agent { get; set; }

        public IEnumerable<TerrainTile> TerrainTiles { get; set; } = Enumerable.Empty<TerrainTile>();

        public IEnumerable<Consumable> Consumables { get; set;} = Enumerable.Empty<Consumable>();

        public IEnumerable<Agent> Agents { get; set; } = Enumerable.Empty<Agent>();

    }
}
