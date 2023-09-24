namespace LandSim.Areas.Agents.Models
{
    public class AgentContextUpdate
    {
        public string PostbackUrl { get; set; } = string.Empty;

        public IEnumerable<AgentContext> AgentContexts { get; set; } = Enumerable.Empty<AgentContext>();

    }
}
