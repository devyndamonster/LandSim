namespace LandSim.Areas.Map.Models
{
    public class SimulationUpdates
    {
        public List<TerrainTile> UpdatedTiles { get; set; } = new List<TerrainTile>();

        public List<Consumable> AddedConsumables { get; set; } = new List<Consumable>();

        public List<Agent> AgentUpdates { get; set; } = new List<Agent>();

        public List<Agent> AddedAgents { get; set; } = new List<Agent>();
    }
}
