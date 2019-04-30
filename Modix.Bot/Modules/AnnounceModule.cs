using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;
using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("announce")]
    [Summary("Makes an announcement")]
    [HelpTags("announce", "$")]
    public class AnnounceModule : ModuleBase
    {
        private IMentionService _mentionService { get; }

        public AnnounceModule(IMentionService mentionService)
        {
            _mentionService = mentionService;
        }

        [Command]
        [Alias("$")]
        [Summary("Makes an announcement in the currnet channel")]
        public async Task MakeAnnouncementAsync([Summary("The role to mention")] IRole role,
            [Remainder, Summary("The message to send as part of the announcement.")]
            string message)
        {
            // Send message
            await _mentionService.MentionRoleAsync(role, Context.Channel, message);

            // Clean up
            await Context.Message.DeleteAsync();
        }
    }
}
