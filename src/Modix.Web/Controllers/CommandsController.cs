using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.Commands;

namespace Modix.Web.Controllers;

[Route("~/api")]
[ApiController]
public class CommandsController : ControllerBase
{
    private readonly ICommandHelpService _commandHelpService;

    public CommandsController(ICommandHelpService commandHelpService)
    {
        _commandHelpService = commandHelpService;
    }

    [HttpGet("commands")]
    public IEnumerable<Module> Commands()
    {
        IEnumerable<Module> ModulesStream()
        {
            var modules = _commandHelpService.GetModuleHelpData();

            var mapped = modules.Select(m =>
            {
                var commands = m.Commands.Select(c =>
                {
                    var parameters = c.Parameters.Select(p => new Parameter(p.Name, p.Summary, p.Options, p.Type, p.IsOptional));

                    return new Command(c.Name, c.Summary, FormatUtilities.CollapsePlurals(c.Aliases), [.. parameters], c.IsSlashCommand);
                });

                return new Module(m.Name, m.Summary, commands);
            });

            foreach (var module in mapped)
            {
                yield return module;
            }
        }

        return ModulesStream();
    }
}
