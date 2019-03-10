using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;

namespace Modix.Controllers
{
    [Route("~/api")]
    public class CommandsController : Controller
    {
        private readonly ICommandHelpService _commandHelpService;

        public CommandsController(ICommandHelpService commandHelpService)
        {
            _commandHelpService = commandHelpService;
        }

        [HttpGet("commands")]
        public IActionResult Commands()
        {
            return Ok(_commandHelpService.GetModuleHelpData());
        }
    }
}
