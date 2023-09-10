namespace LandSim.Areas.Map.Models
{
    public class MapUpdateEvent
    {
        public TerrainTile?[,] TerrainTiles { get; set; } = new TerrainTile?[0, 0];

        public Consumable?[,] Consumables { get; set; } = new Consumable?[0, 0];
    }
}
