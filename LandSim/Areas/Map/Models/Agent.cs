namespace LandSim.Areas.Map.Models
{
    public record Agent : ILocation
    {
        public int AgentId { get; init; }

        public int AgentOwnerId { get; init; }
        
        public int XCoord { get; init; }
        
        public int YCoord { get; init; }

        /// <summary>
        /// Hunger amount, where low amounts are hungry and high amounts are full.
        /// </summary>
        public float Hunger { get; init; } = 1f;

        /// <summary>
        /// Thirst amount, where low amounts are thirsty and high amounts are hydrated.
        /// </summary>
        public float Thirst { get; init; } = 1f;
    }
}
