using System.Text.Json;
using LandSim.Areas.Map.Models;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map
{
    public class MapGenerationRepository
    {
        private readonly MapContext _mapContext;

        public MapGenerationRepository(MapContext mapContext)
        {
            _mapContext = mapContext;
        }

        public void SaveSettings(GenerationSettings settings)
        {
            var currentSettings = _mapContext.GenerationSettings.FirstOrDefault(s => s.GenerationSettingsId == settings.GenerationSettingsId);

            if(currentSettings != null)
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
                .Include(setting => setting.ColorSelectors)
                .ThenInclude(selector => selector.Color)
                .ToList()
                .FirstOrDefault();

            return settings ?? new GenerationSettings();
        }

    }
}
