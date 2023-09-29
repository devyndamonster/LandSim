using LandSim.Areas.Agents.Models;
using System.Numerics;

namespace Agent.Boar
{
    public class BoarService
    {
        public AgentAction GetAction(AgentContext context)
        {
            var closestConsumable = context.Consumables
                .OrderBy(consumable => Math.Abs(consumable.XCoord - context.Agent.XCoord) + Math.Abs(consumable.YCoord - context.Agent.YCoord))
                .FirstOrDefault();

            var agentAction = closestConsumable switch
            {
                null => (AgentActionType)new Random().Next(1, 5),
                var c when c.XCoord == context.Agent.XCoord && c.YCoord == context.Agent.YCoord => AgentActionType.Eat,
                var c when c.XCoord < context.Agent.XCoord => AgentActionType.MoveLeft,
                var c when c.XCoord > context.Agent.XCoord => AgentActionType.MoveRight,
                var c when c.YCoord < context.Agent.YCoord => AgentActionType.MoveUp,
                var c when c.YCoord > context.Agent.YCoord => AgentActionType.MoveDown,
                _ => AgentActionType.None
            };

            return new AgentAction
            {
                AgentId = context.Agent.AgentId,
                ActionType = agentAction
            };
        }
    }
}
