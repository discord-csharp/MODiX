#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.UserMetrics
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class UserMetricsBehavior
        : INotificationHandler<GuildAvailableNotification>,
            INotificationHandler<MessageReceivedNotification>,
            INotificationHandler<UserBannedNotification>,
            INotificationHandler<UserJoinedNotification>,
            INotificationHandler<UserLeftNotification>
    {
        public const string MessageReceivedCounterName
            = "messages_received";

        public const string UserBannedCounterName
            = "user_banned";

        public const string UserCountGaugeName
            = "user_count";

        public const string UserJoinedCounterName
            = "user_joined";

        public const string UserLeftCounterName
            = "user_left";

        public UserMetricsBehavior(
            IDesignatedChannelService designatedChannelService,
            IDesignatedRoleService designatedRoleService,
            ILogger<UserMetricsBehavior> logger)
        {
            _designatedChannelService = designatedChannelService;
            _designatedRoleService = designatedRoleService;
            _logger = logger;
        }

        public Task HandleNotificationAsync(
            GuildAvailableNotification notification,
            CancellationToken cancellationToken)
        {
            var guild = notification.Guild;
            using var logScope = UserMetricsLogMessages.BeginGuildScope(_logger, guild.Id);

            UserMetricsLogMessages.GuildAvailableHandling(_logger);
            UserMetricsLogMessages.GuildAvailableHandled(_logger);

            return Task.CompletedTask;
        }

        public async Task HandleNotificationAsync(
            MessageReceivedNotification notification,
            CancellationToken cancellationToken)
        {
            var message = notification.Message;
            var channel = message.Channel;
            var guild = (channel as IGuildChannel)?.Guild;
            var author = message.Author;

            using var logScope = UserMetricsLogMessages.BeginMessageScope(_logger, guild?.Id, channel.Id, author.Id, message.Id);

            UserMetricsLogMessages.MessageReceivedHandling(_logger);

            if (guild is null)
            {
                UserMetricsLogMessages.IgnoringNonGuildMessage(_logger);
                return;
            }

            if (author.IsBot || author.IsWebhook)
            {
                UserMetricsLogMessages.IgnoringNonHumanMessage(_logger);
                return;
            }

            var isOnTopic = true;
            try
            {
                UserMetricsLogMessages.ChannelParticipationFetching(_logger);
                isOnTopic = await _designatedChannelService.ChannelHasDesignationAsync(
                    guild.Id,
                    channel.Id,
                    DesignatedChannelType.CountsTowardsParticipation,
                    cancellationToken);
                UserMetricsLogMessages.ChannelParticipationFetched(_logger, isOnTopic);
            }
            catch (Exception ex)
            {
                UserMetricsLogMessages.ChannelParticipationFetchFailed(_logger, ex);
            }

            var isRanked = false;
            try
            {
                UserMetricsLogMessages.UserRankFetching(_logger);
                isRanked = await _designatedRoleService.RolesHaveDesignationAsync(
                    guild.Id,
                    ((IGuildUser)author).RoleIds,
                    DesignatedRoleType.Rank,
                    cancellationToken);
                UserMetricsLogMessages.UserRankFetched(_logger, isRanked);
            }
            catch (Exception ex)
            {
                UserMetricsLogMessages.UserRankFetchFailed(_logger, ex);
            }

            UserMetricsLogMessages.MessageReceivedHandled(_logger);
        }

        public Task HandleNotificationAsync(
            UserBannedNotification notification,
            CancellationToken cancellationToken)
        {
            var guild = notification.Guild;
            using var logScope = UserMetricsLogMessages.BeginGuildScope(_logger, guild.Id);

            UserMetricsLogMessages.UserBannedHandling(_logger);
            UserMetricsLogMessages.UserBannedHandled(_logger);

            return Task.CompletedTask;
        }

        public Task HandleNotificationAsync(
            UserJoinedNotification notification,
            CancellationToken cancellationToken)
        {
            var guild = notification.GuildUser.Guild;
            using var logScope = UserMetricsLogMessages.BeginGuildScope(_logger, guild.Id);

            UserMetricsLogMessages.UserJoinedHandling(_logger);
            UserMetricsLogMessages.UserJoinedHandled(_logger);

            return Task.CompletedTask;
        }

        public Task HandleNotificationAsync(
            UserLeftNotification notification,
            CancellationToken cancellationToken)
        {
            var guild = notification.Guild;
            using var logScope = UserMetricsLogMessages.BeginGuildScope(_logger, guild.Id);

            UserMetricsLogMessages.UserLeftHandling(_logger);
            UserMetricsLogMessages.UserLeftHandled(_logger);

            return Task.CompletedTask;
        }

        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IDesignatedRoleService _designatedRoleService;
        private readonly ILogger _logger;
    }
}
