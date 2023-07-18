#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Humanizer;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [ModuleHelp("Emoji Stats", "Commands related to generating statistics on emojis.")]
    [Group("emojistats", "Commands related to generating statistics on emojis.")]
    public class EmojiStatsModule : InteractionModuleBase
    {
        private readonly IEmojiRepository _emojiRepository;

        public EmojiStatsModule(IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }

        [SlashCommand("top", "Gets usage stats for the most popular emojis in the current guild.")]
        public async Task TopEmojiStatsAsync(
            [Summary(description: "How many results to return. Default is 10.")]
            [MinValue(1)]
            [MaxValue(25)]
                int count = 10,
            [Summary(description: "Whether to include all emojis or just custom guild emojis. Default is custom guild emojis.")]
                EmojiTypeFilter emojiTypeFilter = EmojiTypeFilter.CustomEmojisOnly)
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Ascending, count, emojiTypeFilter == EmojiTypeFilter.CustomEmojisOnly);

            await FollowupAsync(embed: embed.Build());
        }

        [SlashCommand("bottom", "Gets usage stats for the least popular emojis in the current guild.")]
        public async Task BottomEmojiStatsAsync(
            [Summary(description: "How many results to return. Default is 10.")]
            [MinValue(1)]
            [MaxValue(25)]
                int count = 10,
            [Summary(description: "Whether to include all emojis or just custom guild emojis. Default is custom guild emojis.")]
                EmojiTypeFilter emojiTypeFilter = EmojiTypeFilter.CustomEmojisOnly)
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Descending, count, emojiTypeFilter == EmojiTypeFilter.CustomEmojisOnly);

            await FollowupAsync(embed: embed.Build());
        }

        [SlashCommand("user", "Gets the usage stats for a specific user.")]
        public async Task UserEmojiStatsAsync(
            [Summary(description: "The user to retrieve stats for.")]
                IUser? user = null,
            [Summary(description: "How many results to return. Default is 10.")]
            [MinValue(1)]
            [MaxValue(25)]
                int count = 10)
        {
            user ??= Context.User;

            var userId = user.Id;
            var guildId = Context.Guild.Id;

            var emojiStats = await _emojiRepository.GetEmojiStatsAsync(guildId, SortDirection.Ascending, count, userId: userId);
            var userTotalUses = await _emojiRepository.GetGuildStatsAsync(guildId, userId);

            var numberOfDays = Math.Max((DateTime.UtcNow - userTotalUses.OldestTimestamp).Days, 1);

            var sb = new StringBuilder();
            BuildEmojiStatString(sb, userTotalUses.TotalUses, emojiStats, (emoji) => (double)emoji.Uses / numberOfDays);

            var totalEmojiUsesPerDay = (double)userTotalUses.TotalUses / numberOfDays;

            await FollowupAsync(embed: new EmbedBuilder()
                .WithAuthor($"{user.GetDisplayName()} - Emoji statistics", user.GetDefiniteAvatarUrl())
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithFooter($"{"unique emoji".ToQuantity(userTotalUses.UniqueEmojis)} used {"time".ToQuantity(userTotalUses.TotalUses)} ({totalEmojiUsesPerDay:0.0}/day) since {userTotalUses.OldestTimestamp:yyyy-MM-dd}")
                .Build());
        }

        [SlashCommand("emoji", "Gets usage stats for a specific emoji.")]
        public async Task EmojiStatsAsync(
            [Summary(description: "The emoji to retrieve information about.")]
                IEmote emoji)
        {
            var asEmote = emoji as Emote;

            var ephemeralEmoji = EphemeralEmoji.FromRawData(emoji.Name, asEmote?.Id, asEmote?.Animated ?? false);
            var guildId = Context.Guild.Id;

            var emojiStats = await _emojiRepository.GetEmojiStatsAsync(guildId, ephemeralEmoji);

            if (emojiStats.Uses == 0)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("Unknown Emoji")
                    .WithDescription($"The emoji \"{ephemeralEmoji.Name}\" has never been used in this server.")
                    .WithColor(Color.Red)
                    .Build());

                return;
            }

            var guildStats = await _emojiRepository.GetGuildStatsAsync(guildId);

            var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(ephemeralEmoji)
                    ? ephemeralEmoji.ToString()
                    : "❔";

            var percentUsage = 100 * (double)emojiStats.Uses / guildStats.TotalUses;
            if (double.IsNaN(percentUsage))
                percentUsage = 0;

            var emojiCreated = ephemeralEmoji.CreatedAt ?? guildStats.OldestTimestamp;
            var numberOfDays = Math.Max((DateTimeOffset.UtcNow - emojiCreated).Days, 1);
            var perDay = (double)emojiStats.Uses / numberOfDays;

            var sb = new StringBuilder(emojiFormatted);

            if (ephemeralEmoji.Id != null)
                sb.Append($" (`:{ephemeralEmoji.Name}:`)");

            sb.AppendLine()
                .AppendLine($"• {"use".ToQuantity(emojiStats.Uses)}")
                .AppendLine($"• {percentUsage:0.0}% of all emoji uses")
                .AppendLine($"• {perDay:0.0/day}");

            if (emojiStats.TopUserId != default)
                sb.AppendLine($"• Top user: {MentionUtils.MentionUser(emojiStats.TopUserId)} ({"use".ToQuantity(emojiStats.TopUserUses)})");

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString());

            await FollowupAsync(embed: embed.Build());
        }

        private async Task<EmbedBuilder> BuildEmojiStatEmbedAsync(SortDirection sortDirection, int count, bool guildOnly)
        {
            var guildId = Context.Guild.Id;

            var emojiFilter = guildOnly
                ? Context.Guild.Emotes.Select(x => x.Id)
                : Enumerable.Empty<ulong>();

            var emojiStats = await _emojiRepository.GetEmojiStatsAsync(guildId, sortDirection, count, emojiIds: emojiFilter);
            var guildStats = await _emojiRepository.GetGuildStatsAsync(guildId, emojiIds: emojiFilter);

            var sb = new StringBuilder();

            BuildEmojiStatString(sb, guildStats.TotalUses, emojiStats, (emoji) =>
            {
                var emojiCreated = emoji.Emoji.CreatedAt ?? guildStats.OldestTimestamp;
                var numberOfDays = Math.Max((DateTimeOffset.UtcNow - emojiCreated).Days, 1);
                return (double)emoji.Uses / numberOfDays;
            });

            var daysSinceOldestEmojiUse = Math.Max((DateTimeOffset.UtcNow - guildStats.OldestTimestamp).Days, 1);
            var totalEmojiUsesPerDay = (double)guildStats.TotalUses / daysSinceOldestEmojiUse;

            return new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithFooter($"{"unique emoji".ToQuantity(guildStats.UniqueEmojis)} used {"time".ToQuantity(guildStats.TotalUses)} ({totalEmojiUsesPerDay:0.0}/day) since {guildStats.OldestTimestamp:yyyy-MM-dd}");
        }

        private void BuildEmojiStatString(
            StringBuilder builder,
            int totalUses,
            IReadOnlyCollection<EmojiUsageStatistics> emojiStats,
            Func<EmojiUsageStatistics, double> perDayStat)
        {
            foreach (var emojiStat in emojiStats)
            {
                var emoji = emojiStat.Emoji;
                var canAccess = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(emoji);

                var emojiFormatted = canAccess
                    ? emoji.ToString()
                    : "❔";

                var percentUsage = 100 * (double)emojiStat.Uses / totalUses;
                if (double.IsNaN(percentUsage))
                    percentUsage = 0;

                var perDay = perDayStat(emojiStat);

                builder.Append($"{emojiStat.Rank}.")
                    .Append($" {emojiFormatted}")
                    .Append($" ({"use".ToQuantity(emojiStat.Uses)})")
                    .Append($" ({percentUsage:0.0}%),")
                    .Append($" {perDay:0.0/day}")
                    .Append(canAccess ? string.Empty : $" ({Format.Url($":{emoji.Name}:", emoji.Url)})")
                    .AppendLine();
            }
        }
    }

    public enum EmojiTypeFilter
    {
        CustomEmojisOnly,
        AllEmojis,
    }
}
