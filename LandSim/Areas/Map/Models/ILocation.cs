namespace LandSim.Areas.Map.Models
{
    public interface ILocation
    {
        public int XCoord { get; init; }
        
        public int YCoord { get; init; }
    }

    public static class ILocationExtensions
    {
        public static bool IsAt(this ILocation location, ILocation other)
        {
            return location.XCoord == other.XCoord && location.YCoord == other.YCoord;
        }
    }
}

