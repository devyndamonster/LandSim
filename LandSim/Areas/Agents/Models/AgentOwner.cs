namespace LandSim.Areas.Agents.Models
{
    public class AgentOwner
    {
        public int AgentOwnerId { get; set; }

        public string Key { get; set; } = string.Empty;

        public string PostbackUrl { get; set; } = string.Empty;
    }
}
