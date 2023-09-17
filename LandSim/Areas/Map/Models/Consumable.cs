namespace LandSim.Areas.Map.Models
{
    public record Consumable : ILocation
    {
        public int XCoord { get; init; }

        public int YCoord { get; init; }

        public int ConsumableId { get; init; }

    }
}
