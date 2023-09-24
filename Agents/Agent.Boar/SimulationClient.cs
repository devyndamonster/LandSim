using LandSim.Areas.Agents.Models;

namespace Agent.Boar
{
    public class SimulationClient
    {
        private readonly HttpClient _client;

        public SimulationClient(HttpClient client)
        {
            _client = client;
        }

        public async Task SendAgentActionUpdates(IEnumerable<AgentAction> agentActionUpdates, string postbackUrl)
        {
            await _client.PostAsJsonAsync(postbackUrl, agentActionUpdates);
        }
    }
}
