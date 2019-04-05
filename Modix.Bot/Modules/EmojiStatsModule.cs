using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Group("emojistats")]
    [Summary("Commands related to generating statistics on emojis.")]
    public class EmojiStatsModule : ModuleBase
    {
        private readonly IEmojiRepository _emojiRepository;

        public EmojiStatsModule(IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }

        [Command("all top")]
        [Alias("all")]
        [Summary("Gets usage stats for the top 10 emojis in the current guild.")]
        public async Task TopEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Ascending);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("all bottom")]
        [Summary("Gets usage stats for the bottom 10 emojis in the current guild.")]
        public async Task BottomEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Descending);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("top")]
        [Alias("")]
        [Summary("Gets usage stats for the top 10 emojis in the current guild.")]
        public async Task TopGuildOnlyEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Ascending, guildOnly: true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bottom")]
        [Summary("Gets usage stats for the bottom 10 emojis in the current guild.")]
        public async Task BottomGuildOnlyEmojiStatsAsync()
        {
            var embed = await BuildEmojiStatEmbedAsync(SortDirection.Descending, guildOnly: true);

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

            var emojiStats = await _emojiRepository.GetEmojiStatsAsync(guildId, ephemeralEmoji);

            if (emojiStats == default || emojiStats.Uses == 0)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Unknown Emoji")
                    .WithDescription($"The emoji \"{ephemeralEmoji.Name}\" has never been used in this server.")
                    .WithColor(Color.Red)
                    .Build());

                return;
            }

            var emojiStats30 = await _emojiRepository.GetEmojiStatsAsync(guildId, ephemeralEmoji, TimeSpan.FromDays(30));
            var guildStats = await _emojiRepository.GetGuildStatsAsync(guildId);

            var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(ephemeralEmoji)
                    ? ephemeralEmoji.ToString()
                    : "❔";

            var percentUsage = 100 * (double)emojiStats.Uses / guildStats.TotalUses;
            if (double.IsNaN(percentUsage))
                percentUsage = 0;

            var numberOfDays = Math.Clamp((DateTime.Now - guildStats.OldestTimestamp).Days, 1, 30);
            var perDay = (double)emojiStats30.Uses / numberOfDays;

            var sb = new StringBuilder(emojiFormatted);

            if (ephemeralEmoji.Id != null)
                sb.Append($" (`:{ephemeralEmoji.Name}:`)");

            sb.AppendLine()
                .AppendLine($"• {"use".ToQuantity(emojiStats.Uses)}")
                .AppendLine($"• {percentUsage.ToString("0.0")}% of all emoji uses")
                .AppendLine($"• {perDay.ToString("0.0/day")}");

            if (emojiStats.TopUserId != default)
                sb.AppendLine($"• Top user: {MentionUtils.MentionUser(emojiStats.TopUserId)} ({"use".ToQuantity(emojiStats.TopUserUses)})");

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString());

            await ReplyAsync(embed: embed.Build());
        }

        private async Task<EmbedBuilder> BuildEmojiStatEmbedAsync(SortDirection sortDirection, bool guildOnly = false)
        {
            var guildId = Context.Guild.Id;

            var emojiFilter = guildOnly
                ? Context.Guild.Emotes.Select(x => x.Id)
                : Enumerable.Empty<ulong>();

            var emojiStats = await _emojiRepository.GetEmojiStatsAsync(guildId, sortDirection, 10, emojiIds: emojiFilter);
            var emojiStats30 = await _emojiRepository.GetEmojiStatsAsync(guildId, sortDirection, 10, TimeSpan.FromDays(30), emojiIds: emojiFilter);
            var guildStats = await _emojiRepository.GetGuildStatsAsync(guildId, emojiIds: emojiFilter);

            var numberOfDays = Math.Clamp((DateTime.Now - guildStats.OldestTimestamp).Days, 1, 30);

            var sb = new StringBuilder();

            foreach (var emojiStat in emojiStats)
            {
                var emoji = emojiStat.Emoji;
                var canAccess = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(emoji);

                var emojiFormatted = canAccess
                    ? emoji.ToString()
                    : "❔";

                var percentUsage = 100 * (double)emojiStat.Uses / guildStats.TotalUses;
                if (double.IsNaN(percentUsage))
                    percentUsage = 0;

                var uses30 = emojiStats30.First(x => x.Emoji.Equals(emoji)).Uses;
                var perDay = (double)uses30 / numberOfDays;

                sb.Append($"{emojiStat.Rank}.")
                    .Append($" {emojiFormatted}")
                    .Append($" ({"use".ToQuantity(emojiStat.Uses)})")
                    .Append($" ({percentUsage.ToString("0.0")}%),")
                    .Append($" {perDay.ToString("0.0/day")}")
                    .Append(canAccess ? string.Empty : $" ({Format.Url($":{emoji.Name}:", emoji.Url)})")
                    .AppendLine();
            }

            var daysSinceOldestEmojiUse = Math.Max((DateTime.Now - guildStats.OldestTimestamp).Days, 1);
            var totalEmojiUsesPerDay = (double)guildStats.TotalUses / daysSinceOldestEmojiUse;

            return new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithFooter($"{"unique emoji".ToQuantity(guildStats.UniqueEmojis)} used {"time".ToQuantity(guildStats.TotalUses)} ({totalEmojiUsesPerDay.ToString("0.0")}/day) since {guildStats.OldestTimestamp.ToString("yyyy-MM-dd")}");
        }
    }
}
