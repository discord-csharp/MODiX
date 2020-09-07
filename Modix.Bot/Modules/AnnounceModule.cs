using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;
using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("Announcements")]
    [Summary("Makes an announcement")]
    [HelpTags("announce", "$")]
    public class AnnounceModule : ModuleBase
    {
        private readonly IMentionService _mentionService;

        public AnnounceModule(IMentionService mentionService)
        {
            _mentionService = mentionService;
        }

        [Command("announce")]
        [Alias("$")]
        [Summary("Makes an announcement in the current channel")]
        public async Task MakeAnnouncementAsync([Summary("The role to mention")] IRole role,
            [Summary("The message to send as part of the announcement.")] [Remainder]
            string message) =>
            await _mentionService.MentionRole(role, Context.Channel, message);
    }
}
