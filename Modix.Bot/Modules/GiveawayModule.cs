using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;

using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
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
        public GiveawayModule(
            IGiveawayService giveawayService,
            IDesignatedChannelService designatedChannelService)
        {
            _giveawayService = giveawayService;
            _designatedChannelService = designatedChannelService;
        }

        [Command("choose")]
        [Alias("pick")]
        [Summary("Randomly chooses a winner for the supplied giveaway.")]
        public async Task ChooseAsync(
            [Summary("The giveaway message from which users will be drawn.")]
                IUserMessage message,
            [Summary("How many winners to choose.")]
                int count = 1)
        {
            var winnersResult = await _giveawayService.GetWinnersAsync(message, count);

            if (winnersResult.IsError)
            {
                await ReplyAsync(winnersResult.Error);
                return;
            }

            var anyGiveawayLogChannels = await _designatedChannelService.AnyDesignatedChannelAsync(Context.Guild.Id, DesignatedChannelType.GiveawayLog);
            if (!anyGiveawayLogChannels)
            {
                await ReplyAsync($"There are no {DesignatedChannelType.GiveawayLog} channels to log the giveaway results to.");
                return;
            }

            var mentions = winnersResult.WinnerIds.Humanize(id => MentionUtils.MentionUser(id));
            var response = $"Congratulations, {mentions}! You've won!";

            await _designatedChannelService.SendToDesignatedChannelsAsync(Context.Guild, DesignatedChannelType.GiveawayLog, response);
        }

        private readonly IGiveawayService _giveawayService;
        private readonly IDesignatedChannelService _designatedChannelService;
    }
}
