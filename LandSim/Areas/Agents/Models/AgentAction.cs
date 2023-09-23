namespace LandSim.Areas.Agents.Models
{
    public class AgentAction
    {
        public int AgentActionId { get; set; }

        public int AgentId { get; set; }

        public AgentActionType ActionType { get; set; }
    }
}
