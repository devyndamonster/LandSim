using LandSim.Areas.Map.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LandSim.Areas.Map.Services
{
    public class BackgroundSimulationService : BackgroundService
    {
        private readonly ILogger<BackgroundSimulationService> _logger;
        private readonly IServiceProvider _services;

        public BackgroundSimulationService(ILogger<BackgroundSimulationService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using(var scope = _services.CreateScope())
            {
                var mapContext = scope.ServiceProvider.GetRequiredService<MapContext>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var tile = await mapContext.TerrainTiles.FirstAsync();

                    if (tile.TerrainType == TerrainType.Water) tile.TerrainType = TerrainType.Soil;
                    else if (tile.TerrainType == TerrainType.Soil) tile.TerrainType = TerrainType.Rock;
                    else if (tile.TerrainType == TerrainType.Rock) tile.TerrainType = TerrainType.Sand;
                    else if (tile.TerrainType == TerrainType.Sand) tile.TerrainType = TerrainType.Water;

                    _logger.LogInformation($"Changed tile to {tile.TerrainType}");
                    
                    await mapContext.SaveChangesAsync();

                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
