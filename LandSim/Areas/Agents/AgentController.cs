using LandSim.Areas.Agents.Models;
using LandSim.Database;
using Microsoft.AspNetCore.Mvc;

namespace LandSim.Areas.Agents
{
    [ApiController]
    [Route("agents")]
    public class AgentController : ControllerBase
    {
        private readonly MapRepository _mapRepository;

        public AgentController(MapRepository mapRepository)
        {
            _mapRepository = mapRepository;
        }

        [HttpPost("actions")]
        public async Task<IActionResult> SetAgentActions([FromBody] IEnumerable<AgentAction> actions)
        {
            foreach(var action in actions)
            {
                await _mapRepository.SetAgentAction(action);
            }

            return Ok();
        }
    }
}
