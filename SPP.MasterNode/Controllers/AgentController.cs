using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPP.Domain.Data;
using SPP.Domain.Entities;
using SPP.Domain.Repositories;
using SPP.MasterNode.Services;
using SPP.MasterNode.Models; // Import models here
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SPP.MasterNode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRepository<AgentEntity> _agentRepository;

        public AgentController(AppDbContext context, IRepository<AgentEntity> agentRepository)
        {
            _context = context;
            _agentRepository = agentRepository;
        }

        /// <summary>Get agent by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return NotFound($"Agent with ID {id} not found");

            return Ok(agent);
        }

        /// <summary>Validate Agent ID format and region</summary>
        [HttpPost("validate/{agentId}")]
        public async Task<IActionResult> ValidateAgentId([FromRoute] string agentId)
        {
            if (string.IsNullOrWhiteSpace(agentId))
                return BadRequest("AgentId is required.");

            var parts = agentId.Split('-');

            if (parts.Length != 2 || parts[1].Length != 3 || !int.TryParse(parts[1], out _))
                return BadRequest("AgentId format is invalid. Expected: [region_code]-[3-digit-number]");

            var regionCode = parts[0];

            var exists = await _context.Regions.AnyAsync(r => r.RegionCode == regionCode);
            if (!exists)
                return BadRequest($"Region code '{regionCode}' is invalid.");

            return Ok(new { message = "AgentId is valid." });
        }

        /// <summary>Send command to all registered worker nodes</summary>
        [HttpPost("send-command")]
        public async Task<IActionResult> SendCommand(
            [FromBody] SendCommandRequest request,
            [FromServices] WorkerRegistryService registry,
            [FromServices] IHttpClientFactory clientFactory)
        {
            var http = clientFactory.CreateClient();
            var workers = registry.GetAllWorkers();

            foreach (var worker in workers)
            {
                var command = new UnifiedCommand
                {
                    Type = request.Command,
                    Payload = "read-file"
                };

                var response = await http.PostAsJsonAsync(worker.Url, command);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Failed to send command to {worker.Url}");
                }
            }

            return Ok("Command sent to all workers.");
        }

        /// <summary>Receive data from a worker node after processing</summary>
        [HttpPost("receive-data")]
        public IActionResult ReceiveData([FromBody] WorkerData data)
        {
            Console.WriteLine($"✅ Received data from {data.WorkerId}: {data.FileData}");
            return Ok("Data received");
        }
    }
}
