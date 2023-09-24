using LandSim.Areas.Agents.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Boar
{
    [ApiController]
    [Route("boar")]
    public class BoarController : ControllerBase
    {
        private readonly ILogger<BoarController> _logger;
        private readonly BoarService _boarService;
        private readonly SimulationClient _simulationClient;

        public BoarController(ILogger<BoarController> logger, BoarService boarService, SimulationClient simulationClient)
        {
            _logger = logger;
            _boarService = boarService;
            _simulationClient = simulationClient;
        }

        [HttpPost("map")]
        public IActionResult PostMapUpdate([FromBody] AgentContextUpdate update)
        {
            Task.Run(async () =>
            {
                var actions = update.AgentContexts.Select(_boarService.GetAction);

                await _simulationClient.SendAgentActionUpdates(actions, update.PostbackUrl);
            });

            return Ok();
        }
    }
}