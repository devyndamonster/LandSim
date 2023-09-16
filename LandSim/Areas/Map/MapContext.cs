using LandSim.Areas.Map.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map
{
    public class MapContext : DbContext
    {
        public DbSet<GenerationSettings> GenerationSettings { get; set; }

        public DbSet<TerrainSelector> TerrainSelectors { get; set; }
        
        public DbSet<TerrainTile> TerrainTiles { get; set; }

        public DbSet<Consumable> Consumables { get; set; }

        public DbSet<Agent> Agents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LandSim.db");
        }

    }
}
