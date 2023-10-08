using LandSim.Areas.Agents.Models;
using LandSim.Database;
using System.Text;

namespace LandSim.Areas.Agents
{
    public class AgentOwnerClient
    {
        private readonly HttpClient _client;
        private readonly MapRepository _mapRepository;

        public AgentOwnerClient(HttpClient client, MapRepository mapRepository)
        {
            _client = client;
            _mapRepository = mapRepository;
        }

        public async Task SendSimulationUpdate(string agentUrl, IEnumerable<AgentContext> agents)
        {
            await _client.PostAsJsonAsync($"{agentUrl}/map", new AgentContextUpdate
            { 
                AgentContexts = agents, 
                PostbackUrl = "http://localhost:39343/agents/actions"
            });
        }

        public async Task<string> GetDecodedShortTermMemory(Agent agent)
        {
            var agentOwner = await _mapRepository.GetAgentOwner(agent.AgentOwnerId);

            var memoryString = Base64UrlEncode(agent.ShortTermMemory);
            var response = await _client.GetAsync($"{agentOwner.PostbackUrl}/short-term-memory?compressedMemory={memoryString}");
            return await response.Content.ReadAsStringAsync();
        }

        private static string Base64UrlEncode(string data)
        {
            return data.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

    }
}
