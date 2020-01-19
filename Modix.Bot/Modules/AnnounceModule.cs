using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Modix.Services.CommandHelp;
using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("Announcements")]
    [Summary("Makes an announcement")]
    [HelpTags("announce", "$")]
    public class AnnounceModule : ModuleBase
    {
        private readonly IMediator _mediator;

        public AnnounceModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command("announce")]
        [Alias("$")]
        [Summary("Makes an announcement in the current channel")]
        public async Task MakeAnnouncementAsync([Summary("The role to mention")] IRole role,
            [Summary("The message to send as part of the announcement.")]
            [Remainder]
            string message)
        {
            if (await _mediator.Send(new MentionCommand(role, Context.Channel, message)))
            {
                await Context.Message.DeleteAsync();
            }
        }
    }
}
