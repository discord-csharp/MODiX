using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("Attachment Blacklist")]
    [Summary("Retrieve information related to the attachment blacklist functionality.")]
    public class AttachmentBlacklistModule : ModuleBase
    {
        [Command("attachment blacklist")]
        [Summary("Retrieves the list of blacklisted attachment file extensions.")]
        public async Task GetBlacklistAsync()
        {
            var blacklistBuilder = new StringBuilder()
                .AppendLine($"{Format.Bold("Blacklisted Extensions")}:")
                .Append("```")
                .AppendJoin(", ", AttachmentBlacklistBehavior.BlacklistedExtensions.OrderBy(d => d))
                .Append("```");

            await ReplyAsync(blacklistBuilder.ToString());
        }
    }
}
