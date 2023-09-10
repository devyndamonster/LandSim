using LandSim.Extensions;

namespace LandSim.Areas.Map.Models
{
    public class WorldData
    {
        public TerrainTile?[,] TerrainTiles { get; set; }

        public Consumable?[,] Consumables { get; set; }

        public Bounds Bounds { get; set; }

        public WorldData(Bounds bounds)
        {
            Bounds = bounds;
            TerrainTiles = new TerrainTile?[bounds.SizeX, bounds.SizeY];
            Consumables = new Consumable?[bounds.SizeX, bounds.SizeY];
        }
        
        public WorldData(TerrainTile[] terrainTiles)
        {
            Bounds = Bounds.FromLocations(terrainTiles);
            TerrainTiles = MapLocationsToBoundedGrid(terrainTiles, Bounds);
            Consumables = new Consumable?[Bounds.SizeX, Bounds.SizeY];
        }

        public WorldData(TerrainTile[] terrainTiles, Consumable[] consumables)
        {
            Bounds = Bounds.FromLocations(terrainTiles);
            TerrainTiles = MapLocationsToBoundedGrid(terrainTiles, Bounds);
            Consumables = MapLocationsToBoundedGrid(consumables, Bounds);
        }

        protected WorldData(TerrainTile?[,] terrainTiles, Consumable?[,] consumables, Bounds bounds)
        {
            TerrainTiles = terrainTiles; 
            Consumables = consumables;
            Bounds = bounds;
        }
        
        private TLocation?[,] MapLocationsToBoundedGrid<TLocation>(TLocation[] locations, Bounds bounds) where TLocation : ILocation
        {
            var locationArray = new TLocation[bounds.SizeX, bounds.SizeY];
            
            foreach (var location in locations)
            {
                var indexX = location.XCoord - bounds.MinX;
                var indexY = location.YCoord - bounds.MinY;

                if(locationArray.IsInArray(indexX, indexY))
                {
                    locationArray[indexX, indexY] = location;
                }
            }

            return locationArray;
        }

        public WorldData Clone()
        {
            var terrainTiles = new TerrainTile?[Bounds.SizeX, Bounds.SizeY];
            for (var x = 0; x < TerrainTiles.GetLength(0); x++)
            {
                for (var y = 0; y < TerrainTiles.GetLength(1); y++)
                {
                    terrainTiles[x, y] = TerrainTiles[x, y]?.Clone();
                }
            }

            var consumables = new Consumable?[Bounds.SizeX, Bounds.SizeY];
            for (var x = 0; x < Consumables.GetLength(0); x++)
            {
                for (var y = 0; y < Consumables.GetLength(1); y++)
                {
                    consumables[x, y] = Consumables[x, y]?.Clone();
                }
            }

            return new WorldData(terrainTiles, consumables, Bounds);
        }
    }
}
