using Microsoft.AspNetCore.Mvc;
using SPP.MasterNode.Models;
using SPP.MasterNode.Services;
using System.Threading.Tasks;

namespace SPP.MasterNode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerNodeController : ControllerBase
    {
        private readonly WorkerRegistryService _workerRegistry;
        private readonly ILogger<WorkerNodeController> _logger;

        public WorkerNodeController(
            WorkerRegistryService workerRegistry,
            ILogger<WorkerNodeController> logger)
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



        [HttpPost("processing-completed")]
        public IActionResult ProcessingCompleted([FromBody] string message)
        {
            Console.WriteLine($"[Worker] Processing of the file completed: {message}");
            return Ok("Received");
        }

        [HttpPost("statistics")]
        public IActionResult ReceiveStatistics([FromBody] WorkerStatistics stats){
            Console.WriteLine($"[Worker] Processed: {stats.TotalAgentsProcessed}, Errors: {stats.ErrorsCount}, Time: {stats.ProcessingTime}");
            return Ok("Statistics received");
        }

        [HttpPost("info")]
        public IActionResult ReceiveInfo([FromBody] WorkerNode info)
        {
            Console.WriteLine($"[Worker] Info processed: {info.Id}, {info.Url}, {info.LastHeartbeat}, {info.Status}, {info.RegisteredAt}");
            return Ok("info received");
        }

    }

    public class WorkerRegistrationRequest
    {
        public string Url { get; set; } = string.Empty;
    }
}