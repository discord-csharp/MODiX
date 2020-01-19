using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using MediatR;
using Modix.Services.CommandHelp;
using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("Mentioning")]
    [Summary("Commands related to mentioning roles.")]
    [HelpTags("mentions", "@")]
    public class MentionModule : ModuleBase
    {
        private readonly IMediator _mediator;

        public MentionModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command("mention")]
        [Alias("@")]
        [Summary("Mentions the supplied role.")]
        public async Task MentionAsync(
            [Summary("The role that the user is attempting to mention.")]
            IRole role,
            [Summary("Message to provide to mentionees. The 'message' argument is ignored by the command.")] [Remainder]
            string message = null) =>
                await _mediator.Send(new MentionCommand(role, Context.Channel, message));
    }
}
