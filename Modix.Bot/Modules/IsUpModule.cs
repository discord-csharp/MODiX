#nullable enable
using System.Threading.Tasks;
using Discord.Commands;
using MediatR;
using Modix.Services.CommandHelp;
using Modix.Services.IsUp;

namespace Modix.Bot.Modules
{
    [Name("Isup")]
    [Summary("Detects if a website has an outage")]
    [HelpTags("isup")]
    public class IsUpModule : ModuleBase
    {
        private readonly IMediator _mediator;

        public IsUpModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command("isup")]
        public async Task IsUp([Summary("Url to ping")] string url)
        {
            await _mediator.Send(new IsUpCommand(Context.Message, url));
        }
    }
}
