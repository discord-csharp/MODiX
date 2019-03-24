using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Modix.Services.EmojiStats;

namespace Modix.Modules
{
    public class EmojiStatsModule : ModuleBase
    {
        private readonly IEmojiStatsService _emojiStatsService;

        public EmojiStatsModule(IEmojiStatsService emojiStatsService)
        {
            _emojiStatsService = emojiStatsService;
        }

        [Command("emojistats")]
        [Summary("Gets usage stats for all emojis in the current guild or a specific emoji if provided.")]
        public async Task Emojistats(
            [Summary("The emote to retrieve information about, if any.")]
                IEmote emote = null)
        {
            var guildId = Context.Guild.Id;

            var emojiUsageAllTime = await _emojiStatsService.GetEmojiSummaries(emote, guildId, null);
            var emojiUsage30 = await _emojiStatsService.GetEmojiSummaries(emote, guildId, TimeSpan.FromDays(30));

            var emojiCountsAllTime = _emojiStatsService
                .GetCountFromSummary(emojiUsageAllTime)
                .OrderByDescending(x => x.Value).ToArray();

            var emojiCounts30 = _emojiStatsService.GetCountFromSummary(emojiUsage30);

            var embed = new EmbedBuilder();
            var totalEmojiUsage = _emojiStatsService.GetTotalEmojiUseCount(emojiCountsAllTime);
            var oldestTimestamp = _emojiStatsService.GetOldestSummaryTimeStamp(emojiUsage30);

            var numberOfDays = Math.Max((DateTime.UtcNow - oldestTimestamp).Days, 1);

            for (int i = 0; i < emojiCountsAllTime.Length; i++)
            {
                var (emoji, count) = emojiCountsAllTime[i];
                var emojiFormatted = Format.Url(emoji.ToString(), emoji.Url);
                var percentUsage = (100 * (double) count / totalEmojiUsage).ToString("0.00");

                double usageLast30 = 0;

                if (emojiCounts30.TryGetValue(emoji, out var countLast30))
                {
                    usageLast30 = (double) countLast30 / numberOfDays;
                }

                embed.Description += $"{i+1}. {emojiFormatted} ({"use".ToQuantity(count)}) ({percentUsage}%), {usageLast30.ToString("0.00/day")}{Environment.NewLine}";
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
