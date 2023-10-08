using LandSim.Areas.Agents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

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

        [HttpGet("short-term-memory")]
        public IActionResult GetDecodedShortTermMemory([FromQuery] string compressedMemory)
        {
            var memoryString = Base64UrlDecode(compressedMemory);
            var decodedMemory = ShortTermMemory.FromCompressedString(memoryString);
            var jsonString = JsonSerializer.Serialize(decodedMemory, new JsonSerializerOptions { WriteIndented = true });
            return Ok(jsonString);
        }

        private static string Base64UrlDecode(string encodedData)
        {
            var decoded = encodedData.Replace('-', '+').Replace('_', '/');
            switch (decoded.Length % 4)
            {
                case 2: decoded += "=="; break;
                case 3: decoded += "="; break;
            }
            return decoded;
        }

    }
}