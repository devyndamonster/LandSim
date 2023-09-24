using LandSim.Areas.Agents.Models;

namespace LandSim.Areas.Agents
{
    public class AgentOwnerClient
    {
        private readonly HttpClient _client;

        public AgentOwnerClient(HttpClient client)
        {
            _client = client;
        }

        public async Task SendSimulationUpdate(string postbackUrl, IEnumerable<AgentContext> agents)
        {
            await _client.PostAsJsonAsync(postbackUrl, agents);
        }

    }
}
