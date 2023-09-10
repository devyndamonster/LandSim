namespace LandSim.Areas.Map.Models
{
    public class Consumable : ILocation
    {
        public int XCoord { get; set; }

        public int YCoord { get; set; }

        public int ConsumableId { get; set; }

        public Consumable Clone()
        {
            return new Consumable
            {
                XCoord = XCoord,
                YCoord = YCoord,
                ConsumableId = ConsumableId
            };
        }
    }
}
