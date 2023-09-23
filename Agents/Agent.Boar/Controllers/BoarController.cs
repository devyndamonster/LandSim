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
        public async Task<IActionResult> PostMapUpdate()
        {
            return Ok();
        }
    }
}