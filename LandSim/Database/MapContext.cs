using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Generation.Models;
using LandSim.Areas.Map.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Database
{
    public class MapContext : DbContext
    {
        public DbSet<GenerationSettings> GenerationSettings { get; set; }

        public DbSet<TerrainSelector> TerrainSelectors { get; set; }
        
        public DbSet<TerrainTile> TerrainTiles { get; set; }

        public DbSet<Consumable> Consumables { get; set; }

        public DbSet<Agent> Agents { get; set; }

        public DbSet<AgentAction> AgentActions { get; set; }

        public DbSet<AgentOwner> AgentOwner { get; set; }

        public DbSet<SimulationConfig> SimulationConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LandSim.db");
        }

    }
}
