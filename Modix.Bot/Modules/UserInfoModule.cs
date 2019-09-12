using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Emoji;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Images;
using Modix.Services.Moderation;
using Modix.Services.Promotions;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("User Information")]
    [Summary("Retrieves information and statistics about the supplied user.")]
    [HelpTags("userinfo", "info")]
    public class UserInfoModule : ModuleBase
    {
        //optimization: UtcNow is slow and the module is created per-request
        private readonly DateTime _utcNow = DateTime.UtcNow;

        public UserInfoModule(
            ILogger<UserInfoModule> logger,
            IUserService userService,
            IModerationService moderationService,
            IAuthorizationService authorizationService,
            IMessageRepository messageRepository,
            IEmojiRepository emojiRepository,
            IPromotionsService promotionsService,
            IImageService imageService,
            IOptions<ModixConfig> config)
        {
            _log = logger ?? new NullLogger<UserInfoModule>();
            _userService = userService;
            _moderationService = moderationService;
            _authorizationService = authorizationService;
            _messageRepository = messageRepository;
            _emojiRepository = emojiRepository;
            _promotionsService = promotionsService;
            _imageService = imageService;
            _config = config.Value;
        }

        private readonly ILogger<UserInfoModule> _log;
        private readonly IUserService _userService;
        private readonly IModerationService _moderationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageRepository _messageRepository;
        private readonly IEmojiRepository _emojiRepository;
        private readonly IPromotionsService _promotionsService;
        private readonly IImageService _imageService;
        private readonly ModixConfig _config;

        [Command("info")]
        [Alias("ompf")]
        [Summary("Retrieves information about the supplied user, or the current user if one is not provided.")]
        public async Task GetUserInfoAsync(
            [Summary("The user to retrieve information about, if any.")]
                [Remainder] DiscordUserOrMessageAuthorEntity user = null)
        {
            var userId = user?.UserId ?? Context.User.Id;

            var timer = Stopwatch.StartNew();

            var userInfo = await _userService.GetUserInformationAsync(Context.Guild.Id, userId);

            if (userInfo == null)
            {
                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, we don't have any data for that user - and we couldn't find any, either.")
                    .AddField("User Id", userId)
                    .Build());

                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("**\u276F User Information**");
            builder.AppendLine("ID: " + userId);
            builder.AppendLine("Profile: " + MentionUtils.MentionUser(userId));

            var embedBuilder = new EmbedBuilder()
                .WithUserAsAuthor(userInfo)
                .WithTimestamp(_utcNow);

            var avatar = userInfo.GetDefiniteAvatarUrl();

            embedBuilder.ThumbnailUrl = avatar;
            embedBuilder.Author.IconUrl = avatar;

            ValueTask<Color> colorTask = default;

            if ((userInfo.GetAvatarUrl(size: 16) ?? userInfo.GetDefaultAvatarUrl()) is { } avatarUrl)
            {
                colorTask = _imageService.GetDominantColorAsync(new Uri(avatarUrl));
            }

            var moderationReadTask = _authorizationService.HasClaimsAsync(Context.User as IGuildUser, AuthorizationClaim.ModerationRead);
            var userRankTask = _messageRepository.GetGuildUserParticipationStatistics(Context.Guild.Id, userId);
            var messagesByDateTask = _messageRepository.GetGuildUserMessageCountByDate(Context.Guild.Id, userId, TimeSpan.FromDays(30));
            var messageCountsByChannelTask = _messageRepository.GetGuildUserMessageCountByChannel(Context.Guild.Id, userId, TimeSpan.FromDays(30));
            var emojiCountsTask = _emojiRepository.GetEmojiStatsAsync(Context.Guild.Id, SortDirection.Ascending, 1, userId: userId);
            var promotionsTask = _promotionsService.GetPromotionsForUserAsync(Context.Guild.Id, userId);

            embedBuilder.WithColor(await colorTask);

            var moderationRead = await moderationReadTask;

            if (userInfo.IsBanned)
            {
                builder.AppendLine("Status: **Banned** \\🔨");

                if (moderationRead)
                {
                    builder.AppendLine($"Ban Reason: {userInfo.BanReason}");
                }
            }
            else
            {
                builder.AppendLine($"Status: {userInfo.Status.Humanize()}");
            }

            if (userInfo.FirstSeen is DateTimeOffset firstSeen)
                builder.AppendLine($"First Seen: {FormatUtilities.FormatTimeAgo(_utcNow, firstSeen)}");

            if (userInfo.LastSeen is DateTimeOffset lastSeen)
                builder.AppendLine($"Last Seen: {FormatUtilities.FormatTimeAgo(_utcNow, lastSeen)}");

            try
            {
                await AddParticipationToEmbedAsync(userId, builder, await userRankTask, await messagesByDateTask, await messageCountsByChannelTask, await emojiCountsTask);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occured while retrieving a user's message count.");
            }

            AddMemberInformationToEmbed(userInfo, builder);
            AddPromotionsToEmbed(builder, await promotionsTask);

            if (moderationRead)
            {
                await AddInfractionsToEmbedAsync(userId, builder);
            }

            embedBuilder.Description = builder.ToString();

            timer.Stop();
            embedBuilder.WithFooter(footer => footer.Text = $"Completed after {timer.ElapsedMilliseconds} ms");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        private void AddMemberInformationToEmbed(EphemeralUser member, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("**\u276F Member Information**");

            if (!string.IsNullOrEmpty(member.Nickname))
            {
                builder.AppendLine("Nickname: " + member.Nickname);
            }

            builder.AppendLine($"Created: {FormatUtilities.FormatTimeAgo(_utcNow, member.CreatedAt)}");

            if (member.JoinedAt is DateTimeOffset joinedAt)
            {
                builder.AppendLine($"Joined: {FormatUtilities.FormatTimeAgo(_utcNow, joinedAt)}");
            }

            if (member.RoleIds?.Count > 0)
            {
                var roles = member.RoleIds.Select(x => member.Guild.Roles.Single(y => y.Id == x))
                    .Where(x => x.Id != x.Guild.Id) // @everyone role always has same ID than guild
                    .ToArray();

                if (roles.Length > 0)
                {
                    Array.Sort(roles); // Sort by position: lowest positioned role is first
                    Array.Reverse(roles); // Reverse the sort: highest positioned role is first

                    builder.Append($"{"Role".ToQuantity(roles.Length, ShowQuantityAs.None)}: ");
                    builder.AppendLine(roles.Select(r => r.Mention).Humanize());
                }
            }
        }

        private async Task AddInfractionsToEmbedAsync(ulong userId, StringBuilder builder)
        {
            // https://modix.gg/infractions?subject=1234
            var url = new UriBuilder(_config.WebsiteBaseUrl)
            {
                Path = "/infractions",
                Query = $"subject={userId}"
            }.RemoveDefaultPort().ToString();

            builder.AppendLine();
            builder.AppendLine($"**\u276F Infractions [See here]({url})**");

            if (!(Context.Channel as IGuildChannel).IsPublic())
            {
                var counts = await _moderationService.GetInfractionCountsForUserAsync(userId);
                builder.AppendLine(FormatUtilities.FormatInfractionCounts(counts));
            }
            else
            {
                builder.AppendLine("Infractions cannot be listed in public channels.");
            }
        }

        private async Task AddParticipationToEmbedAsync(
            ulong userId,
            StringBuilder builder,
            GuildUserParticipationStatistics userRank,
            IReadOnlyDictionary<DateTime, int> messagesByDate,
            IReadOnlyDictionary<ulong, int> messageCountsByChannel,
            IReadOnlyCollection<EmojiUsageStatistics> emojiCounts)
        {
            var lastWeek = _utcNow - TimeSpan.FromDays(7);

            var weekTotal = 0;
            var monthTotal = 0;
            foreach (var kvp in messagesByDate)
            {
                if (kvp.Key >= lastWeek)
                {
                    weekTotal += kvp.Value;
                }

                monthTotal += kvp.Value;
            }

            builder.AppendLine();
            builder.AppendLine("**\u276F Guild Participation**");

            if (userRank?.Rank > 0)
            {
                builder.AppendFormat("Rank: {0} {1}\n", userRank.Rank.Ordinalize(), GetParticipationEmoji(userRank));
            }

            var weekParticipation = "Last 7 days: " + weekTotal + " messages";
            if (weekTotal > 0 && monthTotal > 0)
            {
                var percentage = (int)((decimal)weekTotal / monthTotal * 100);
                weekParticipation += string.Format(" ({0}%)", percentage);
            }

            builder.AppendLine(weekParticipation);
            builder.AppendLine("Last 30 days: " + monthTotal + " messages");

            if (monthTotal > 0)
            {
                builder.AppendFormat(
                    "Avg. per day: {0} messages (top {1} percentile)\n",
                    decimal.Round(userRank.AveragePerDay, 3),
                    userRank.Percentile.Ordinalize());

                try
                {
                    foreach (var kvp in messageCountsByChannel.OrderByDescending(x => x.Value))
                    {
                        var channel = await Context.Guild.GetChannelAsync(kvp.Key);

                        if (channel.IsPublic())
                        {
                            builder.AppendLine($"Most active channel: {MentionUtils.MentionChannel(channel.Id)} ({kvp.Value} messages)");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.LogDebug(ex, "Unable to get the most active channel for {UserId}.", userId);
                }
            }

            if (emojiCounts.Any())
            {
                var favoriteEmoji = emojiCounts.First();

                var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(favoriteEmoji.Emoji)
                    ? Format.Url(favoriteEmoji.Emoji.ToString(), favoriteEmoji.Emoji.Url)
                    : $"{Format.Url("❔", favoriteEmoji.Emoji.Url)} (`{favoriteEmoji.Emoji.Name}`)";

                builder.AppendLine($"Favorite emoji: {emojiFormatted} ({"time".ToQuantity(favoriteEmoji.Uses)})");
            }
        }

        private string GetParticipationEmoji(GuildUserParticipationStatistics stats)
        {
            if (stats.Percentile == 100 || stats.Rank == 1)
            {
                return "🥇";
            }
            else if (stats.Percentile == 99 || stats.Rank == 2)
            {
                return "🥈";
            }
            else if (stats.Percentile == 98 || stats.Rank == 3)
            {
                return "🥉";
            }
            else if (stats.Percentile >= 95 && stats.Percentile < 98)
            {
                return "🏆";
            }

            return string.Empty;
        }

        private void AddPromotionsToEmbed(StringBuilder builder, IReadOnlyCollection<PromotionCampaignSummary> promotions)
        {
            if (promotions.Count == 0)
                return;

            builder.AppendLine();
            builder.AppendLine(Format.Bold("\u276F Promotion History"));

            foreach (var promotion in promotions.OrderByDescending(x => x.CloseAction.Id).Take(5))
            {
                builder.AppendLine($"• {MentionUtils.MentionRole(promotion.TargetRole.Id)} {FormatUtilities.FormatTimeAgo(_utcNow, promotion.CloseAction.Created)}");
            }
        }
    }
}
