using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;

namespace Modix.Modules
{
    [Name("Wrong channel")]
    [Summary("Notifies a user when the #lowlevel channel has been used incorrectly.")]
    [HelpTags("wrongchannel")]
    public class WrongChannelModule : ModuleBase
    {
        private const long Help0ChannelId = 169726586931773440;

        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public WrongChannelModule(IAutoRemoveMessageService autoRemoveMessageService)
        {
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [Command("wrongchannel")]
        [Summary("Notifies a user when the #lowlevel channel has been used incorrectly.")]
        public async Task PingUserAboutWrongChannelAsync(
            [Summary("The user to retrieve information about, if any.")]
            [Remainder] DiscordUserOrMessageAuthorEntity user = null)
        {
            if (user is null)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Missing user id")
                    .WithColor(Color.Red)
                    .WithDescription("You need to specify a target user to ping. Use this comment as \"!wrongchannel <USER_ID>\".");

                await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                    Context.User,
                    embed,
                    async (embedBuilder) => await ReplyAsync(embed: embedBuilder.Build()));

                return;
            }

            var builder = new StringBuilder();

            builder.AppendLine($"Hey {MentionUtils.MentionUser(user.UserId)}! 👋");
            builder.AppendLine($"You'll want to ask in another channel, such as {MentionUtils.MentionUser(Help0ChannelId)}. This channel, as per the description, is for:");
            builder.AppendLine("> \"Low level programming discussion around the very intricate implementation details of the C# language, the speeds, quirks and optimisations\"");
            builder.AppendLine("Lowlevel in this context refers to being \"close to hardware\" or about more advanced hardware details in general.");
            builder.AppendLine("Thank you!");

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription(builder.ToString())
                .Build());

            await Context.Message.DeleteAsync();
        }
    }
}
