using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;
using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("Mentioning")]
    [Summary("Commands related to mentioning roles.")]
    [HelpTags("mentions", "@")]
    public class MentionModule : ModuleBase
    {
        private readonly IMentionService _mentionService;

        public MentionModule(IMentionService mentionService)
        {
            _mentionService = mentionService;
        }

        [Command("mention")]
        [Alias("@")]
        [Summary("Mentions the supplied role.")]
        public async Task MentionAsync(
            [Summary("The role that the user is attempting to mention.")]
            IRole role,
            [Summary("Message to provide to mentionees. The 'message' argument is ignored by the command.")] [Remainder]
            string message = null) =>
                await _mentionService.MentionRole(role, Context.Channel, message);
    }
}
