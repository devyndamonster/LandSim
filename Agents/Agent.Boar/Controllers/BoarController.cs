using LandSim.Areas.Agents.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Boar.Controllers
{
    [ApiController]
    [Route("boar")]
    public class BoarController : ControllerBase
    {
        private readonly ILogger<BoarController> _logger;

        public BoarController(ILogger<BoarController> logger)
        {
            _logger = logger;
        }

        [HttpPost("map")]
        public async Task<IActionResult> PostMapUpdate([FromBody] IEnumerable<AgentContext> agents)
        {
            return Ok();
        }
    }
}