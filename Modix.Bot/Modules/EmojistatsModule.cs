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

            var emojiUsageAllTime = await _emojiStatsService.GetEmojiSummaries(emote, guildId, false);
            var emojiUsage30 = await _emojiStatsService.GetEmojiSummaries(emote, guildId, true);

            var emojiCountsAllTime = _emojiStatsService
                .GetCountFromSummary(emojiUsageAllTime)
                .OrderByDescending(x => x.Value);

            var emojiCounts30 = _emojiStatsService.GetCountFromSummary(emojiUsage30);

            var embed = new EmbedBuilder();
            var totalEmojiUsage = _emojiStatsService.AggregateUsage(emojiCountsAllTime);

            var oldestTimestamp = _emojiStatsService.GetOldestSummaryTimeStamp(emojiUsage30);

            var divider = (DateTime.UtcNow - oldestTimestamp).Days;
            divider = divider == 0 ? 1 : divider;

            for (int i = 0; i < emojiCountsAllTime.Count(); i++)
            {
                var kvp = emojiCountsAllTime.ElementAt(i);
                var emojiFormatted = Format.Url(kvp.Key.ToString(), kvp.Key.Url);
                var usage = kvp.Value;
                var percentUsage = (100 * (double) usage / totalEmojiUsage).ToString("0.00");

                double usageLast30 = 0;

                if (emojiCounts30.TryGetValue(kvp.Key, out var val))
                {
                    usageLast30 = (double)val / divider;
                }

                embed.Description += $"{i+1}. {emojiFormatted} ({"use".ToQuantity(usage)}) ({percentUsage}%), {usageLast30.ToString("0.00/day")}\n";
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
