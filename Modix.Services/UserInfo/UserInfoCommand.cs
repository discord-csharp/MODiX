#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Models.Promotions;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Extensions;
using Modix.Services.Images;
using Modix.Services.Utilities;

namespace Modix.Services.UserInfo
{
    public class UserInfoCommand : IRequest
    {
        public UserInfoCommand(IMessage message, ulong guildId, ulong userId)
        {
            Message = message;
            GuildId = guildId;
            UserId = userId;
        }
        public ulong UserId { get; }
        public ulong GuildId { get; }
        public IMessage Message { get; }
    }

    public class UserInfoCommandHandler : AsyncRequestHandler<UserInfoCommand>
    {
        private readonly ModixContext _modixContext;
        private readonly IDiscordClient _discordClient;
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly IImageService _imageService;

        public UserInfoCommandHandler(ModixContext modixContext, IDiscordClient discordClient,
            IAutoRemoveMessageService autoRemoveMessageService, IImageService imageService)
        {
            _modixContext = modixContext;
            _discordClient = discordClient;
            _autoRemoveMessageService = autoRemoveMessageService;
            _imageService = imageService;
        }

        protected override async Task Handle(UserInfoCommand request,
            CancellationToken cancellationToken)
        {
            var guild = await _discordClient.GetGuildAsync(request.GuildId);
            var discordUser = await _discordClient.GetUserAsync(request.UserId);

            var modixUser = await _modixContext
                .GuildUsers
                .Where(x => x.UserId == request.UserId)
                .Where(x => x.GuildId == request.GuildId)
                .Select(x => new
                {
                    x.FirstSeen,
                    x.LastSeen,
                    Infractions = x.Infractions
                        .Where(d => d.DeleteActionId == null)
                        .Select(d => new
                        {
                            Type = d.Type,
                            Date = d.CreateAction.Created,
                            Reason = d.Reason,
                        }).ToList(),
                    Promotions = x.PromotionCampaigns
                        .Where(d => d.Outcome == PromotionCampaignOutcome.Accepted)
                        .Where(d => d.CloseActionId != null)
                        .Select(d => new
                        {
                            Date = d.CloseAction!.Created,
                            TargetRoleId = d.TargetRoleId,
                        }).ToList(),
                })
                .FirstOrDefaultAsync();

            if (discordUser is null
                && modixUser is null)
            {
                // If we have nothing from Discord or from the database,
                // we'll need to assume this user doesn't exist

                var embed = new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, we don't have any data for that user - and we couldn't find any, either.")
                    .AddField("User Id", request.UserId);

                await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                    request.Message.Author,
                    embed,
                    async (builder) =>
                        await request.Message.ReplyAsync(string.Empty, embed: builder.Build()));
                return;
            }

            var userInfoBuilder = UserInfoEmbedBuilderHelper.Create();

            const int NumberOfDaysInPeriodToCount = 30;
            var startingThreshold = DateTimeOffset.UtcNow.AddDays(-NumberOfDaysInPeriodToCount);
            var weekThreshold = startingThreshold.AddDays(-7);

            var messages = await _modixContext
                    .Messages
                    .Where(x => x.GuildId == request.GuildId)
                    .Where(x => x.AuthorId == request.UserId)
                    .Where(x => x.Channel.DesignatedChannelMappings.Any(d =>
                        d.Type == DesignatedChannelType.CountsTowardsParticipation))
                    .Where(x => x.Timestamp > startingThreshold)
                    .GroupBy(x => x.Timestamp.DateTime)
                    .Select(x => new
                    {
                        ChannelId = x.Key,
                    }).ToListAsync(cancellationToken: cancellationToken);

            var rankedUsers = await _modixContext
                .GuildUsers
                .Where(x => x.Messages.Where(f => f.Channel.DesignatedChannelMappings
                    .Any(d => d.Type == DesignatedChannelType.CountsTowardsParticipation))
                    .Any(d => d.Timestamp > startingThreshold))
                .Where(x => x.GuildId == request.GuildId)
                .Select(x =>
                new
                {
                    x.UserId,
                    NumberOfMessages = x.Messages
                    .Where(e => e.Channel.DesignatedChannelMappings
                        .Any(d => d.Type == DesignatedChannelType.CountsTowardsParticipation))
                    .Count(d => d.Timestamp > startingThreshold),
                })
                .OrderByDescending(x => x.NumberOfMessages)
                .ToListAsync(cancellationToken: cancellationToken);

            //var totalNumberOfMessagesInPeriod = messages.Sum(x => x.NumberOfMessages);
            //var totalNumberOfMessagesIn7Days = messages.Where(x => x.Date.Date > weekThreshold.Date).Sum(d => d.NumberOfMessages);
            //var averageMessagesSent = totalNumberOfMessagesInPeriod / NumberOfDaysInPeriodToCount;

            //var rankedUser = rankedUsers.Single(x => x.UserId == request.UserId);
            //var userRank = rankedUsers.IndexOf(rankedUser) + 1;

            //var percentile = (1 - (userRank / rankedUsers.Count)) * 100;

            userInfoBuilder
                .WithId(request.UserId)
                .WithClickableMention(request.UserId);

            if (modixUser.Infractions.Any(x => x.Type == InfractionType.Ban))
            {
                var banReason = modixUser
                    .Infractions
                    .Where(x => x.Type == InfractionType.Ban)
                    .OrderByDescending(x => x.Date)
                    .Select(x => x.Reason)
                    .First();

                userInfoBuilder.WithBan(banReason);
            }

            if (discordUser is IGuildUser guildUser)
            {
                userInfoBuilder
                    .WithStatus(discordUser.Status)
                    .WithFirstAndLastSeen(modixUser.FirstSeen, modixUser.LastSeen)
                    .WithMemberInformation(guildUser);
            }

            //userInfoBuilder
            //    .WithParticipationInPeriod(userRank,
            //totalNumberOfMessagesInPeriod,
            //totalNumberOfMessagesIn7Days,
            //averageMessagesSent,
            //percentile);

            //var mostPopularChannel = messages
            //    .OrderByDescending(x => x.NumberOfMessages)
            //    .Single();

            //var channel = await guild.GetChannelAsync(mostPopularChannel.ChannelId);

            //if (channel.IsPublic())
            //{
            //    userInfoBuilder.WithChannel(mostPopularChannel.ChannelId,
            //        mostPopularChannel.NumberOfMessages);
            //}

            var promotionHistory = modixUser
                .Promotions
                .Select(promotion => (promotion.TargetRoleId, promotion.Date))
                .ToList();

            if (promotionHistory.Any())
            {
                userInfoBuilder.WithPromotions(promotionHistory);
            }

            var content = userInfoBuilder.Build();

            var avatar = discordUser.GetDefiniteAvatarUrl();

            var colorTask = await _imageService.GetDominantColorAsync(new Uri(avatar));

            var embedBuilder = new EmbedBuilder()
                .WithUserAsAuthor(discordUser)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithThumbnailUrl(avatar)
                .WithDescription(content)
                .WithColor(colorTask)
                .WithFooter("TODO");

            embedBuilder.Author.IconUrl = avatar;

            var usersAbleToRemove = new[]
            {
                discordUser,
                request.Message.Author,
            };

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(
                usersAbleToRemove,
                embedBuilder,
                async (builder) => await request.Message.ReplyAsync(string.Empty, builder.Build()));
        }
    }

    public class UserInfoEmbedBuilderHelper
    {
        private readonly StringBuilder _content = new StringBuilder();
        private readonly DateTimeOffset _nowUtc = DateTimeOffset.UtcNow;

        public static UserInfoEmbedBuilderHelper Create()
        {
            return new UserInfoEmbedBuilderHelper();
        }

        private UserInfoEmbedBuilderHelper()
        {
            _content.AppendLine("**\u276F User Information**");
        }

        public UserInfoEmbedBuilderHelper WithId(ulong userId)
        {
            _content.AppendLine("ID: " + userId);
            return this;
        }

        public UserInfoEmbedBuilderHelper WithClickableMention(ulong userId)
        {
            _content.AppendLine("Profile: " + MentionUtils.MentionUser(userId));
            return this;
        }

        public UserInfoEmbedBuilderHelper WithFirstAndLastSeen(DateTimeOffset firstSeen, DateTimeOffset lastSeen)
        {
            _content.AppendLine($"First Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, firstSeen)}");
            _content.AppendLine($"Last Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, lastSeen)}");

            return this;
        }

        public UserInfoEmbedBuilderHelper WithStatus(UserStatus status)
        {
            _content.AppendLine($"Status: {status.Humanize()}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithBan(string reason)
        {
            _content.AppendLine($"🔨\\ **Banned**: {reason}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithParticipationInPeriod(int rank,
            int numberOfMessagesIn30Days,
            int numberOfMessagesIn7Days,
            double averagePerDay,
            int percentile)
        {
            _content.AppendLine("**\u276F Guild Participation**");
            _content.AppendFormat($"Rank: {rank} {GetParticipationEmoji(rank)}");
            _content.AppendFormat($"Last 7 days: {numberOfMessagesIn7Days}");
            _content.AppendFormat($"Last 30 days: {numberOfMessagesIn30Days}");
            _content.AppendFormat($"Average/day: {averagePerDay} (top {percentile})");

            return this;
        }

        public UserInfoEmbedBuilderHelper WithChannel(ulong channelId,
            int numberOfMessagesInChannel)
        {
            _content.AppendLine($"Most active channel: {MentionUtils.MentionChannel(channelId)} ({numberOfMessagesInChannel} messages)");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithMemberInformation(IGuildUser user)
        {
            _content.AppendLine("**\u276F Member Information**");

            if (!string.IsNullOrEmpty(user.Nickname))
            {
                _content.AppendLine("Nickname: " + user.Nickname);
            }

            _content.AppendLine($"Created: {FormatUtilities.FormatTimeAgo(_nowUtc, user.CreatedAt)}");

            if (user.JoinedAt != null)
            {
                _content.AppendLine($"Joined: {FormatUtilities.FormatTimeAgo(_nowUtc, user.JoinedAt.Value)}");
            }

            if (user.RoleIds?.Count > 0)
            {
                var roles = user.RoleIds.Select(x => user.Guild.Roles.Single(y => y.Id == x))
                    .Where(x => x.Id != x.Guild.Id)
                    .OrderByDescending(role => role)
                    .ToArray();

                if (roles.Any())
                {
                    _content.Append($"{"Role".ToQuantity(roles.Length, ShowQuantityAs.None)}: ");
                    _content.AppendLine(roles.Select(r => r.Mention).Humanize());
                }
            }

            return this;
        }

        public UserInfoEmbedBuilderHelper WithPromotions(List<(ulong targetRoleId, DateTimeOffset date)> promotions)
        {
            _content.AppendLine(Format.Bold("\u276F Promotion History"));

            foreach (var promotion in promotions.OrderByDescending(x => x.date))
            {
                _content.AppendLine($"• {MentionUtils.MentionRole(promotion.targetRoleId)} {FormatUtilities.FormatTimeAgo(_nowUtc, promotion.date)}");
            }

            return this;
        }

        public string Build()
        {
            return _content.ToString();
        }

        private string GetParticipationEmoji(int rank) =>
            rank switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => "🏆",
            };
    }
}
