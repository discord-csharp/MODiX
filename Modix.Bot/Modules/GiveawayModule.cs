using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;

using Modix.Services.CommandHelp;
using Modix.Services.Giveaways;

namespace Modix.Bot.Modules
{
    [Name("Giveaways")]
    [Summary("Host timed or untimed giveaways.")]
    [Group("giveaway")]
    [Alias("giveaways")]
    [HelpTags("giveaways")]
    public class GiveawayModule : ModuleBase
    {
        public GiveawayModule(IGiveawayService giveawayService)
        {
            _giveawayService = giveawayService;
        }

        [Command("choose")]
        [Alias("pick")]
        [Summary("Randomly chooses a winner for the supplied giveaway.")]
        public async Task ChooseAsync(
            [Summary("The giveaway message from which users will be drawn.")]
                DiscordUserMessage message,
            [Summary("How many winners to choose.")]
                int count = 1)
        {
            var winnersResult = await _giveawayService.GetWinnersAsync(message.ToUserMessage(), count);

            if (winnersResult.IsError)
            {
                await ReplyAsync(winnersResult.Error);
                return;
            }

            var mentions = winnersResult.Winners.Humanize(id => MentionUtils.MentionUser(id));
            var response = $"Congratulations, {mentions}! You've won!";

            await ReplyAsync(response);
        }

        private readonly IGiveawayService _giveawayService;
    }
}
