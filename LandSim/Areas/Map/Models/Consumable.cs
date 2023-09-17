namespace LandSim.Areas.Map.Models
{
    public record Consumable : ILocation
    {
        public int XCoord { get; set; }

        public int YCoord { get; set; }

        public int ConsumableId { get; set; }

    }
}
