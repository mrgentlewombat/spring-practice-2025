using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace CentralApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inject the database context
        public AgentController(AppDbContext context)
        {
            _context = context;
        }

        // Validate the AgentId format and region code
        [HttpPost("validate/{agentId}")]
        public async Task<IActionResult> ValidateAgentId([FromRoute] string agentId)
        {
            // Check if agentId is empty
            if (string.IsNullOrWhiteSpace(agentId))
                return BadRequest("AgentId is required.");

            // Split agentId into parts (e.g. "RO-123")
            var parts = agentId.Split('-');

            // Check format: must be two parts, second part must be 3 digits
            if (parts.Length != 2 || parts[1].Length != 3 || !int.TryParse(parts[1], out _))
            {
                return BadRequest("AgentId format is invalid. Expected: [region_code]-[3-digit-number]");
            }

            var regionCode = parts[0];

            // Check if region code exists in the database
            var exists = await _context.Regions.AnyAsync(r => r.RegionCode == regionCode);
            if (!exists)
            {
                return BadRequest($"Region code '{regionCode}' is invalid.");
            }

            // AgentId is valid
            return Ok(new { message = "AgentId is valid." });
        }
    }

    // Not used in this example, but can be used for body input
    public record ValidateAgentIdRequest(string AgentId);
}
