namespace LandSim.Areas.Agents.Models
{
    public enum AgentActionType
    {
        None,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        Eat,
    }

    public static class AgentActionTypeExtensions
    {
        public static bool IsMove(this AgentActionType agentActionType)
        {
            return agentActionType is AgentActionType.MoveLeft or AgentActionType.MoveRight or AgentActionType.MoveUp or AgentActionType.MoveDown;
        }
    }
}
