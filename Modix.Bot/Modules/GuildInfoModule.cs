using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Modix.Data.Models;
using Modix.Data.Repositories;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Images;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Guild Information")]
    [Summary("Retrieves information and statistics about the supplied guild.")]
    [HelpTags("guildinfo")]
    public sealed class GuildInfoModule : ModuleBase
    {
        //optimization: UtcNow is slow and the module is created per-request
        private readonly DateTime _utcNow = DateTime.UtcNow;

        public GuildInfoModule(
            IGuildService guildService,
            IMessageRepository messageRepository,
            IEmojiRepository emojiRepository,
            IImageService imageService)
        {
            _guildService = guildService;
            _messageRepository = messageRepository;
            _emojiRepository = emojiRepository;
            _imageService = imageService;
        }

        private readonly IGuildService _guildService;
        private readonly IMessageRepository _messageRepository;
        private readonly IEmojiRepository _emojiRepository;
        private readonly IImageService _imageService;

        [Command("guildinfo")]
        [Alias("serverinfo")]
        [Summary("Retrieves information about the supplied guild, or the current guild if one is not provided.")]
        public async Task GetUserInfoAsync(
            [Summary("The unique Discord snowflake ID of the guild to retrieve information about, if any.")]
                ulong? guildId = null)
        {
            var timer = Stopwatch.StartNew();

            var resolvedGuildId = guildId ?? Context.Guild.Id;

            var guildResult = await _guildService.GetGuildInformationAsync(resolvedGuildId);

            if (guildResult.IsError)
            {
                var errorDescription = guildResult.IsError
                    ? guildResult.Error
                    : "Sorry, we don't have any data for that guild - and we couldn't find any, either.";

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription(errorDescription)
                    .AddField("Guild Id", resolvedGuildId)
                    .Build());

                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(guildResult.Guild.Name, guildResult.Guild.IconUrl)
                .WithThumbnailUrl(guildResult.Guild.IconUrl)
                .WithTimestamp(_utcNow);

            await WithDominantColorAsync(embedBuilder, guildResult.Guild);

            var stringBuilder = new StringBuilder();

            AppendGuildInformation(stringBuilder, guildResult.Guild);

            if (guildResult.Guild is SocketGuild socketGuild)
            {
                await AppendGuildParticipationAsync(stringBuilder, socketGuild);
                AppendMemberInformation(stringBuilder, socketGuild);
            }

            AppendRoleInformation(stringBuilder, guildResult.Guild);

            embedBuilder.WithDescription(stringBuilder.ToString());

            timer.Stop();
            embedBuilder.WithFooter($"Completed after {timer.ElapsedMilliseconds} ms");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        public async Task WithDominantColorAsync(EmbedBuilder embedBuilder, IGuild guild)
        {
            if (guild.IconUrl is { } iconUrl)
            {
                var color = await _imageService.GetDominantColorAsync(new Uri(iconUrl));
                embedBuilder.WithColor(color);
            }
        }

        public void AppendGuildInformation(StringBuilder stringBuilder, IGuild guild)
        {
            stringBuilder
                .AppendLine(Format.Bold("\u276F Guild Information"))
                .AppendLine($"ID: {guild.Id}")
                .AppendLine($"Owner: {MentionUtils.MentionUser(guild.OwnerId)}")
                .AppendLine($"Created: {FormatUtilities.FormatTimeAgo(_utcNow, guild.CreatedAt)}")
                .AppendLine();
        }

        public async Task AppendGuildParticipationAsync(StringBuilder stringBuilder, SocketGuild guild)
        {
            var weekTotal = await _messageRepository.GetTotalMessageCountAsync(guild.Id, TimeSpan.FromDays(7));
            var monthTotal = await _messageRepository.GetTotalMessageCountAsync(guild.Id, TimeSpan.FromDays(30));

            var channelCounts = await _messageRepository.GetTotalMessageCountByChannelAsync(guild.Id, TimeSpan.FromDays(30));
            var orderedChannelCounts = channelCounts.OrderByDescending(x => x.Value);
            var mostActiveChannel = orderedChannelCounts.FirstOrDefault();

            stringBuilder
                .AppendLine(Format.Bold("\u276F Guild Participation"))
                .AppendLine($"Last 7 days: {"message".ToQuantity(weekTotal, "n0")}")
                .AppendLine($"Last 30 days: {"message".ToQuantity(monthTotal, "n0")}")
                .AppendLine($"Avg. per day: {"message".ToQuantity(monthTotal / 30, "n0")}");

            if (!(mostActiveChannel is { }))
            {
                stringBuilder
                    .AppendLine($"Most active channel: {MentionUtils.MentionChannel(mostActiveChannel.Key)} ({"message".ToQuantity(mostActiveChannel.Value, "n0")} in 30 days)");
            }

            var emojiCounts = await _emojiRepository.GetEmojiStatsAsync(guild.Id, SortDirection.Ascending, 1);

            if (emojiCounts.Any())
            {
                var favoriteEmoji = emojiCounts.First();

                var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(favoriteEmoji.Emoji)
                    ? Format.Url(favoriteEmoji.Emoji.ToString(), favoriteEmoji.Emoji.Url)
                    : $"{Format.Url("❔", favoriteEmoji.Emoji.Url)} (`{favoriteEmoji.Emoji.Name}`)";

                stringBuilder.AppendLine($"Favorite reaction: {emojiFormatted} ({"time".ToQuantity(favoriteEmoji.Uses)})");
            }

            stringBuilder.AppendLine();
        }

        public void AppendMemberInformation(StringBuilder stringBuilder, SocketGuild guild)
        {
            var onlineMembers = guild.Users.Count(x => x.Status != UserStatus.Offline);
            var members = guild.Users.Count;
            var bots = guild.Users.Count(x => x.IsBot);
            var humans = members - bots;

            stringBuilder
                .AppendLine(Format.Bold("\u276F Member Information"))
                .AppendLine($"Total member count: {members} ({onlineMembers} online)")
                .AppendLine($"• Humans: {humans}")
                .AppendLine($"• Bots: {bots}")
                .AppendLine();
        }

        public void AppendRoleInformation(StringBuilder stringBuilder, IGuild guild)
        {
            var roles = guild.Roles
                .Where(x => x.Id != guild.EveryoneRole.Id && x.Color != Color.Default)
                .OrderByDescending(x => x.Position)
                .ThenByDescending(x => x.IsHoisted);

            stringBuilder.AppendLine(Format.Bold("\u276F Guild Roles"))
                .AppendLine(roles.Select(x => x.Mention).Humanize())
                .AppendLine();
        }
    }
}
