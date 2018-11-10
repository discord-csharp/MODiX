using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("Attachment Blacklist")]
    public class AttachmentBlacklistModule : ModuleBase
    {
        [Command("attachment blacklist")]
        [Summary("Retrieves the list of blacklisted attachment file extensions")]
        public async Task GetBlacklist()
        {
            await ReplyAsync($"**Blacklisted Extensions**:\n```{string.Join(", ", AttachmentBlacklistBehavior.BlacklistedExtensions.OrderBy(d => d))}```");
        }
    }
}
