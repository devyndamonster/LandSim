using LandSim.Areas.Agents.Models;
using LandSim.Areas.Map.Models;

namespace LandSim.Areas.Simulation.Models
{
    public class SimulationUpdates
    {
        public List<TerrainTile> UpdatedTiles { get; set; } = new List<TerrainTile>();

        public List<Consumable> AddedConsumables { get; set; } = new List<Consumable>();

        public List<Consumable> RemovedConsumables { get; set; } = new List<Consumable>();

        public List<Agent> AgentUpdates { get; set; } = new List<Agent>();

        public List<Agent> AddedAgents { get; set; } = new List<Agent>();

        public List<Agent> RemovedAgents { get; set; } = new List<Agent>();
    }
}
