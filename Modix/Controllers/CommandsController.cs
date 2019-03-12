using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

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
            var modules = _commandHelpService.GetModuleHelpData();

            var mapped = modules.Select(m => new
            {
                Name = m.Name,
                Summary = m.Summary,
                Commands = m.Commands.Select(c => new
                {
                    Name = c.Name,
                    Summary = c.Summary,
                    Aliases = FormatUtilities.CollapsePlurals(c.Aliases),
                    Parameters = c.Parameters,
                }),
            });

            return Ok(mapped);
        }
    }
}
