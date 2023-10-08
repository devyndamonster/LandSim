using LandSim.Areas.Generation.Models;
using LandSim.Database;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Generation.Database
{
    public class GenerationSettingsRepository
    {
        private readonly MapContext _mapContext;

        public GenerationSettingsRepository(MapContext mapContext)
        {
            _mapContext = mapContext;
        }


        public void SaveSettings(GenerationSettings settings)
        {
            var currentSettings = _mapContext.GenerationSettings.FirstOrDefault(s => s.GenerationSettingsId == settings.GenerationSettingsId);

            if (currentSettings != null)
            {
                _mapContext.Update(settings);
            }
            else
            {
                _mapContext.GenerationSettings.Add(settings);
            }

            _mapContext.SaveChanges();
        }

        public GenerationSettings GetSettings()
        {
            var settings = _mapContext.GenerationSettings
                .Include(setting => setting.TerrainSelectors)
                .ToList()
                .FirstOrDefault();

            return settings ?? new GenerationSettings();
        }
    }
}
