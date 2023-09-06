using LandSim.Areas.Map.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map
{
    public class MapContext : DbContext
    {
        public DbSet<GenerationSettings> GenerationSettings { get; set; }

        public DbSet<ColorSelector> ColorSelectors { get; set; }

        public DbSet<Color> Colors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LandSim.db");
        }

    }
}
