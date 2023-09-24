using LandSim.Areas.Agents.Models;

namespace Agent.Boar
{
    public class BoarService
    {
        public AgentAction GetAction(AgentContext context)
        {
            return new AgentAction
            {
                AgentId = context.Agent.AgentId,
                ActionType = (AgentActionType) new Random().Next(0, 4)
            };
        }
    }
}
