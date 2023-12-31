﻿namespace LandSim.Areas.Map.Models
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

        public static int DistanceTo(this ILocation location, ILocation other)
        {
            return Math.Abs(location.XCoord - other.XCoord) + Math.Abs(location.YCoord - other.YCoord);
        }

        public static bool IsNextTo(this ILocation location, ILocation other)
        {
            return location.DistanceTo(other) == 1;
        }

        public static (int deltaX, int deltaY) Difference(this ILocation location, ILocation other)
        {
            return (other.XCoord - location.XCoord, other.YCoord - location.YCoord);
        }
    }
}

