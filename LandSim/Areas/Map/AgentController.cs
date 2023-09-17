using Microsoft.AspNetCore.Mvc;

namespace LandSim.Areas.Map
{
    [ApiController]
    [Route("agents")]
    public class AgentController : ControllerBase
    {
        [HttpPost("actions")]
        public async Task<IActionResult> SetAgentActions()
        {
            return Ok();
        }
    }
}
