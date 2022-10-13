using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Modix.Services.CommandHelp;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [ModuleHelp("Attachment Blacklist", "Retrieve information related to the attachment blacklist functionality.")]
    public class AttachmentBlacklistModule : InteractionModuleBase
    {
        [SlashCommand("attachment-blacklist", "Retrieves the list of blacklisted attachment file extensions.")]
        public async Task GetBlacklistAsync()
        {
            var blacklistBuilder = new StringBuilder()
                .AppendLine($"{Format.Bold("Blacklisted Extensions")}:")
                .Append("```")
                .AppendJoin(", ", AttachmentBlacklistBehavior.BlacklistedExtensions.OrderBy(d => d))
                .Append("```");

            await FollowupAsync(blacklistBuilder.ToString(), allowedMentions: AllowedMentions.None);
        }
    }
}
