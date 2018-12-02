using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Services.Mentions;

namespace Modix.Bot.Modules
{
    [Name("Mentioning")]
    [Summary("Commands related to mentioning roles.")]
    public class MentionModule : ModuleBase
    {
        public MentionModule(IMentionService mentionService)
        {
            MentionService = mentionService;
        }

        [Command("mention")]
        [Alias("@")]
        [Summary("Mentions the supplied role.")]
        public async Task MentionAsync(
            [Summary("The role that the user is attempting to mention.")]
                IRole role)
        {
            if (await MentionService.CanUserMentionAsync(role))
            {
                var mentionContinuation = await MentionService.EnsureMentionable(role);

                await Context.Channel.SendMessageAsync(MentionUtils.MentionRole(role.Id));

                await mentionContinuation();
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Sorry, but you do not have permission to mention the {role.Name} role.");
            }
        }

        [Command("mention")]
        [Alias("@")]
        [Summary("Mentions the supplied role.")]
        public async Task MentionAsync(
            [Summary("The role that the user is attempting to mention.")]
                IRole role,
            [Summary("The message that is being sent with the mention.")]
            [Remainder]
                string message)
            => await MentionAsync(role);

        internal protected IMentionService MentionService { get; }
    }
}
