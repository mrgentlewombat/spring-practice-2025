using Microsoft.AspNetCore.Mvc;
using SPP.MasterNode.Models;
using SPP.MasterNode.Services;
using System.Threading.Tasks;

namespace SPP.MasterNode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerRegistrationController : ControllerBase
    {
        private readonly WorkerRegistryService _workerRegistry;
        private readonly ILogger<WorkerRegistrationController> _logger;

        public WorkerRegistrationController(
            WorkerRegistryService workerRegistry,
            ILogger<WorkerRegistrationController> logger)
        {
            _workerRegistry = workerRegistry;
            _logger = logger;
        }

        [HttpPost("register")]
        public ActionResult<WorkerNode> RegisterWorker([FromBody] WorkerRegistrationRequest request)
        {
            try
            {
                var worker = _workerRegistry.RegisterWorker(request.Url);
                return Ok(worker);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering worker");
                return StatusCode(500, "Internal server error during worker registration");
            }
        }

        [HttpPost("{workerId}/heartbeat")]
        public ActionResult UpdateHeartbeat(string workerId)
        {
            if (_workerRegistry.UpdateHeartbeat(workerId))
            {
                return Ok();
            }
            return NotFound($"Worker with ID {workerId} not found");
        }

        [HttpGet]
        public ActionResult<IEnumerable<WorkerNode>> GetWorkers()
        {
            return Ok(_workerRegistry.GetAllWorkers());
        }

        [HttpGet("{workerId}")]
        public ActionResult<WorkerNode> GetWorker(string workerId)
        {
            var worker = _workerRegistry.GetWorker(workerId);
            if (worker == null)
            {
                return NotFound($"Worker with ID {workerId} not found");
            }
            return Ok(worker);
        }
    }

    public class WorkerRegistrationRequest
    {
        public string Url { get; set; } = string.Empty;
    }
}