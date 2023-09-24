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

        public async Task SendSimulationUpdate(string agentUrl, IEnumerable<AgentContext> agents)
        {
            await _client.PostAsJsonAsync(agentUrl, new AgentContextUpdate
            { 
                AgentContexts = agents, 
                PostbackUrl = "http://localhost:39343/agents/actions"
            });
        }

    }
}
