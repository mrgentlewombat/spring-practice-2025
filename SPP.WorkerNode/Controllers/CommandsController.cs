using Microsoft.AspNetCore.Mvc;
using SPP.WorkerNode.Services;
using System.Collections.Generic;
using System.Linq;

namespace SPP.WorkerNode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly CommandProcessor _commandProcessor;

        public CommandsController(CommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAllCommands()
        {
            return Ok(_commandProcessor.GetRegisteredCommands());
        }
    }
}
