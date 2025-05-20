using Microsoft.AspNetCore.Mvc;
using SPP.MasterNode.Models;
using SPP.MasterNode.Services;

namespace SPP.MasterNode.Controllers
{
    [ApiController]
    [Route("api/worker")]
    public class WorkerController : ControllerBase
    {
        private readonly WorkerRegistryService _registry;

        public WorkerController(WorkerRegistryService registry)
        {
            _registry = registry;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterWorkerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("Url is required.");
            }

            var worker = _registry.RegisterWorker(request.Url);
            return Ok(worker);
        }

        [HttpGet("list")]
        public IActionResult List()
        {
            var workers = _registry.GetAllWorkers();
            return Ok(workers);
        }
    }
}
