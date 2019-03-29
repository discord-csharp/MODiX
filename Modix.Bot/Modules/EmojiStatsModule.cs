﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Services.EmojiStats;
using Modix.Services.Utilities;

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
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Ascending);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bottom")]
        [Summary("Gets usage stats for the bottom 10 emojis in the current guild.")]
        public async Task BottomEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Descending);

            await ReplyAsync(embed: embed.Build());
        }

        //[Command()]
        //[Priority(-10)]
        //[Summary("Gets usage stats for a specific emoji.")]
        //public async Task EmojiStatsAsync(
        //    [Summary("The emoji to retrieve information about.")]
        //        IEmote emoji)
        //{
        //    var asEmote = emoji as Emote;

        //    var ephemeralEmoji = EphemeralEmoji.FromRawData(emoji.Name, asEmote?.Id, asEmote?.Animated ?? false);
        //    var guildId = Context.Guild.Id;

        //    var emojiUsageAllTime = await _emojiStatsService.GetEmojiStatsAsync(guildId, null, ephemeralEmoji);

        //    if (emojiUsageAllTime.Count == 0)
        //    {
        //        await ReplyAsync(embed: new EmbedBuilder()
        //            .WithTitle("Unknown Emoji")
        //            .WithDescription($"The emoji \"{ephemeralEmoji.Name}\" has never been used in this server.")
        //            .WithColor(Color.Red)
        //            .Build());

        //        return;
        //    }

        //    var emojiCountsAllTime = _emojiStatsService
        //        .GetCountsFromSummaries(emojiUsageAllTime)
        //        .OrderByDescending(x => x.Value)
        //        .ToArray();

        //    var emojiUsage30 = await _emojiStatsService.GetEmojiStats(guildId, TimeSpan.FromDays(30), ephemeralEmoji);
        //    var emojiCounts30 = _emojiStatsService.GetCountsFromSummaries(emojiUsage30);

        //    var totalEmojiUsage = _emojiStatsService.GetTotalEmojiUseCount(emojiCountsAllTime);
        //    var oldestTimestamp = _emojiStatsService.GetOldestSummaryTimestamp(emojiUsage30);

        //    var numberOfDays = Math.Max((DateTime.UtcNow - oldestTimestamp).Days, 1);

        //    var (_, count) = emojiCountsAllTime.FirstOrDefault(x =>
        //        ephemeralEmoji.Id is null
        //            ? x.Key.Name == ephemeralEmoji.Name
        //            : x.Key.Id == ephemeralEmoji.Id);

        //    var emojiFormatted = Format.Url(ephemeralEmoji.ToString(), ephemeralEmoji.Url);

        //    var percentUsage = 100 * (double)count / totalEmojiUsage;
        //    if (double.IsNaN(percentUsage))
        //        percentUsage = 0;

        //    var usageLast30 = 0d;

        //    if (emojiCounts30.TryGetValue(ephemeralEmoji, out var countLast30))
        //    {
        //        usageLast30 = (double)countLast30 / numberOfDays;
        //    }

        //    var (topUserId, topUserCount) = emojiUsageAllTime
        //        .GroupBy(x => x.UserId)
        //        .Select(x => (UserId: x.Key, Count: x.Count()))
        //        .OrderByDescending(x => x.Count)
        //        .FirstOrDefault();

        //    var sb = new StringBuilder(emojiFormatted);

        //    if (ephemeralEmoji.Id != null)
        //        sb.Append($" (`:{ephemeralEmoji.Name}:`)");

        //    sb.AppendLine()
        //        .AppendLine($"• {"use".ToQuantity(count)}")
        //        .AppendLine($"• {percentUsage.ToString("0.0")}% of all emoji uses")
        //        .AppendLine($"• {usageLast30.ToString("0.0/day")}");

        //    if (topUserId != default)
        //        sb.AppendLine($"• Top user: {MentionUtils.MentionUser(topUserId)} ({"use".ToQuantity(topUserCount)})");

        //    var embed = new EmbedBuilder()
        //        .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
        //        .WithColor(Color.Blue)
        //        .WithDescription(sb.ToString());

        //    await ReplyAsync(embed: embed.Build());
        //}

        private async Task<EmbedBuilder> BuildEmojiStatEmbedAsync(SortDirection sortDirection)
        {
            var guildId = Context.Guild.Id;

            var emojiStats = await _emojiStatsService.GetEmojiStatsAsync(guildId, sortDirection, 10);
            var emojistats30 = await _emojiStatsService.GetEmojiStatsAsync(guildId, sortDirection, 10, TimeSpan.FromDays(30));
            var guildStats = await _emojiStatsService.GetGuildStatsAsync(guildId);

            var numberOfDays = Math.Clamp((DateTime.Now - guildStats.OldestTimestamp).Days, 1, 30);

            var sb = new StringBuilder();

            foreach (var emojiStat in emojiStats)
            {
                var emoji = emojiStat.Emoji;

                var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(emoji)
                    ? Format.Url(emoji.ToString(), emoji.Url)
                    : Format.Url("❔", emoji.Url);

                var percentUsage = 100 * (double)emojiStat.Uses / guildStats.TotalUses;
                if (double.IsNaN(percentUsage))
                    percentUsage = 0;

                var uses30 = emojistats30.First(x => x.Emoji.Equals(emoji)).Uses;
                var perDay = (double)uses30 / numberOfDays;

                sb.Append($"{emojiStat.Rank}.")
                    .Append($" {emojiFormatted}")
                    .Append($" ({"use".ToQuantity(emojiStat.Uses)})")
                    .Append($" ({percentUsage.ToString("0.0")}%),")
                    .Append($" {perDay.ToString("0.0/day")}")
                    .Append(EmojiUtilities.IsBuiltInEmoji(emoji.Name) ? string.Empty : $" (`:{emoji.Name}:`)")
                    .AppendLine();
            }

            var daysSinceOldestEmojiUse = Math.Max((DateTime.Now - guildStats.OldestTimestamp).Days, 1);
            var totalEmojiUsesPerDay = (double)guildStats.TotalUses / daysSinceOldestEmojiUse;

            return new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithFooter($"{"unique emoji".ToQuantity(guildStats.UniqueEmojis)} used {"time".ToQuantity(guildStats.TotalUses)} ({totalEmojiUsesPerDay.ToString("0.0")}/day) since {guildStats.OldestTimestamp.ToString("d")}");
        }
    }
}
