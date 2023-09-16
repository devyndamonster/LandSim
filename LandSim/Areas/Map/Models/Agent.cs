namespace LandSim.Areas.Map.Models
{
    public class Agent : ILocation
    {
        public int AgentId { get; set; }
        
        public int XCoord { get; set; }
        
        public int YCoord { get; set; }

        /// <summary>
        /// Hunger amount, where low amounts are hungry and high amounts are full.
        /// </summary>
        public float Hunger { get; set; } = 1f;

        /// <summary>
        /// Thirst amount, where low amounts are thirsty and high amounts are hydrated.
        /// </summary>
        public float Thirst { get; set; } = 1f;
    }
}
