using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Services.FileUpload;

namespace Modix.Modules
{
    public class AttachmentBlacklistModule : ModuleBase
    {
        [Command("attachment blacklist")]
        [Summary("Retrieves the list of blacklisted attachment file extensions")]
        public async Task GetBlacklist()
        {
            await ReplyAsync($"**Blacklisted Extensions**:\n```{string.Join(", ", FileUploadBehavior.BlacklistedExtensions.OrderBy(d => d))}```");
        }
    }
}
