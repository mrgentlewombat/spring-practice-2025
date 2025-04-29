using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentralApp.Data;

namespace CentralApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateAgentId([FromBody] ValidateAgentIdRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AgentId))
                return BadRequest("AgentId is required.");

            var parts = request.AgentId.Split('-');
            if (parts.Length != 2 || parts[1].Length != 3 || !int.TryParse(parts[1], out _))
            {
                return BadRequest("AgentId format is invalid. Expected: [region_code]-[3-digit-number]");
            }

            var regionCode = parts[0];

            var exists = await _context.Regions.AnyAsync(r => r.RegionCode == regionCode);
            if (!exists)
            {
                return BadRequest($"Region code '{regionCode}' is invalid.");
            }

            return Ok(new { message = "AgentId is valid." });
        }
    }

    public class ValidateAgentIdRequest
    {
        public string AgentId { get; set; }
    }
}
