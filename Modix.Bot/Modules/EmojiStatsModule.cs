using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Modix.Data.Models.Emoji;
using Modix.Services.EmojiStats;

namespace Modix.Modules
{
    [Group("emojistats")]
    [Summary("Commands related to generating statistics on emojis.")]
    public class EmojiStatsModule : ModuleBase
    {
        private readonly IEmojiStatsService _emojiStatsService;

        public EmojiStatsModule(IEmojiStatsService emojiStatsService)
        {
            _emojiStatsService = emojiStatsService;
        }

        [Command("top")]
        [Alias("")]
        [Summary("Gets usage stats for the top 10 emojis in the current guild.")]
        public async Task TopEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(sortByAscending: false);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bottom")]
        [Summary("Gets usage stats for the bottom 10 emojis in the current guild.")]
        public async Task BottomEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(sortByAscending: true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command()]
        [Priority(-10)]
        [Summary("Gets usage stats for a specific emoji.")]
        public async Task EmojiStatsAsync(
            [Summary("The emoji to retrieve information about.")]
                IEmote emoji)
        {
            var asEmote = emoji as Emote;

            var ephemeralEmoji = EphemeralEmoji.FromRawData(emoji.Name, asEmote?.Id, asEmote?.Animated ?? false);
            var guildId = Context.Guild.Id;

            var emojiUsageAllTime = await _emojiStatsService.GetEmojiSummaries(guildId, null);
            var emojiCountsAllTime = _emojiStatsService
                .GetCountsFromSummaries(emojiUsageAllTime)
                .OrderByDescending(x => x.Value)
                .ToArray();

            var emojiUsage30 = await _emojiStatsService.GetEmojiSummaries(guildId, TimeSpan.FromDays(30));
            var emojiCounts30 = _emojiStatsService.GetCountsFromSummaries(emojiUsage30);

            var totalEmojiUsage = _emojiStatsService.GetTotalEmojiUseCount(emojiCountsAllTime);
            var oldestTimestamp = _emojiStatsService.GetOldestSummaryTimestamp(emojiUsage30);

            var numberOfDays = Math.Max((DateTime.UtcNow - oldestTimestamp).Days, 1);

            var (_, count) = emojiCountsAllTime.FirstOrDefault(x =>
                ephemeralEmoji.Id is null
                    ? x.Key.Name == ephemeralEmoji.Name
                    : x.Key.Id == ephemeralEmoji.Id);

            var emojiFormatted = Format.Url(ephemeralEmoji.ToString(), ephemeralEmoji.Url);

            var percentUsage = 100 * (double)count / totalEmojiUsage;
            if (double.IsNaN(percentUsage))
                percentUsage = 0;

            var usageLast30 = 0d;

            if (emojiCounts30.TryGetValue(ephemeralEmoji, out var countLast30))
            {
                usageLast30 = (double)countLast30 / numberOfDays;
            }

            var comparer = new EphemeralEmoji.EqualityComparer();

            var (topUserId, topUserCount) = emojiUsageAllTime
                .Where(x => comparer.Equals(x.Emoji, ephemeralEmoji))
                .GroupBy(x => x.UserId)
                .Select(x => (UserId: x.Key, Count: x.Count()))
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var sb = new StringBuilder(emojiFormatted);

            if (ephemeralEmoji.Id != null)
                sb.Append($" (`:{ephemeralEmoji.Name}:`)");

            sb.AppendLine()
                .AppendLine($"• {"use".ToQuantity(count)}")
                .AppendLine($"• {percentUsage.ToString("0.0")}% of all emoji uses")
                .AppendLine($"• {usageLast30.ToString("0.0/day")}");

            if (topUserId != default)
                sb.AppendLine($"• Top user: {MentionUtils.MentionUser(topUserId)} ({"use".ToQuantity(topUserCount)})");

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString());

            await ReplyAsync(embed: embed.Build());
        }

        private async Task<EmbedBuilder> BuildEmojiStatEmbedAsync(bool sortByAscending)
        {
            var guildId = Context.Guild.Id;
            var emojiUsageAllTime = await _emojiStatsService.GetEmojiSummaries(guildId, null);

            var allEmojiCounts = _emojiStatsService
                .GetCountsFromSummaries(emojiUsageAllTime);

            var emojiCountsAllTime = (sortByAscending
                ? allEmojiCounts.OrderBy(x => x.Value)
                : allEmojiCounts.OrderByDescending(x => x.Value))
                .ToArray();

            var emojiUsage30 = await _emojiStatsService.GetEmojiSummaries(guildId, TimeSpan.FromDays(30));
            var emojiCounts30 = _emojiStatsService.GetCountsFromSummaries(emojiUsage30);

            var totalEmojiUsage = _emojiStatsService.GetTotalEmojiUseCount(emojiCountsAllTime);
            var oldestTimestamp30 = _emojiStatsService.GetOldestSummaryTimestamp(emojiUsage30);

            var numberOfDays = Math.Max((DateTime.UtcNow - oldestTimestamp30).Days, 1);

            var distinctEmoji = emojiCountsAllTime
                .Select(x => x.Key)
                .Distinct(new EphemeralEmoji.EqualityComparer());

            var sb = new StringBuilder();

            for (var i = 0; i < emojiCountsAllTime.Length && i < 10; i++)
            {
                var (emoji, count) = emojiCountsAllTime[i];
                var emojiFormatted = Format.Url(emoji.ToString(), emoji.Url);

                var percentUsage = 100 * (double)count / totalEmojiUsage;
                if (double.IsNaN(percentUsage))
                    percentUsage = 0;

                var usageLast30 = 0d;
                if (emojiCounts30.TryGetValue(emoji, out var countLast30))
                {
                    usageLast30 = (double)countLast30 / numberOfDays;
                }

                sb.Append($"{i + 1}.")
                    .Append($" {emojiFormatted}")
                    .Append($" ({"use".ToQuantity(count)})")
                    .Append($" ({percentUsage.ToString("0.0")}%),")
                    .AppendLine($" {usageLast30.ToString("0.0/day")}");
            }

            var oldestTimestampAllTime = _emojiStatsService.GetOldestSummaryTimestamp(emojiUsageAllTime);
            var daysSinceOldestEmojiUse = Math.Max((DateTime.UtcNow - oldestTimestampAllTime).Days, 1);
            var totalEmojiUsesPerDay = (double)totalEmojiUsage / daysSinceOldestEmojiUse;

            return new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithFooter($"{"unique emoji".ToQuantity(distinctEmoji.Count())} used {"time".ToQuantity(totalEmojiUsage)} ({totalEmojiUsesPerDay.ToString("0.0")}/day) since {oldestTimestampAllTime.ToString("d")}");
        }
    }
}
