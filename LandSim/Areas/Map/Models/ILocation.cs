namespace LandSim.Areas.Map.Models
{
    public interface ILocation
    {
        public int XCoord { get; set; }
        
        public int YCoord { get; set; }
    }

    public static class ILocationExtensions
    {
        public static bool IsAt(this ILocation location, ILocation other)
        {
            return location.XCoord == other.XCoord && location.YCoord == other.YCoord;
        }
    }
}

