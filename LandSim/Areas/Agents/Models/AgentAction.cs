using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LandSim.Areas.Agents.Models
{
    [Index(nameof(AgentId), IsUnique = true)]
    public class AgentAction
    {
        [Key]
        public int AgentActionId { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public AgentActionType ActionType { get; set; }
    }
}
