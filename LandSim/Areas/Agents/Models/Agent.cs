using LandSim.Areas.Map.Models;
using LandSim.Shared;
using System.ComponentModel.DataAnnotations;

namespace LandSim.Areas.Agents.Models
{
    public record Agent : BaseRecord, ILocation
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

        /// <summary>
        /// Cooldown until agent can reproducte again, with zero being ready to reproduce.
        /// </summary>
        public float ReproductionCooldown { get; init; } = 1f;

        [MaxLength(300)]
        public string ShortTermMemory { get; init; } = string.Empty;

    }
}
