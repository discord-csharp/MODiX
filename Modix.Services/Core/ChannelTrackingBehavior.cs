using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    /// <summary>
    /// Ensures that Discord channel data within the local datastore remains synchronized with the Discord API.
    /// </summary>
    public class ChannelTrackingBehavior
        : INotificationHandler<ChannelCreatedNotification>,
            INotificationHandler<ChannelUpdatedNotification>,
            INotificationHandler<GuildAvailableNotification>,
            INotificationHandler<JoinedGuildNotification>
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelTrackingBehavior"/> object, with the given injected dependencies.
        /// </summary>
        public ChannelTrackingBehavior(
            IChannelService channelService)
        {
            _channelService = channelService;
        }

        /// <inheritdoc />
        public Task HandleNotificationAsync(ChannelCreatedNotification notification, CancellationToken cancellationToken = default)
            => notification.Channel is ITextChannel textChannel
                ? _channelService.TrackChannelAsync(textChannel.Name, textChannel.Id, textChannel.GuildId, cancellationToken)
                : Task.CompletedTask;

        /// <inheritdoc />
        public Task HandleNotificationAsync(ChannelUpdatedNotification notification, CancellationToken cancellationToken = default)
            => notification.NewChannel is ITextChannel textChannel
                ? _channelService.TrackChannelAsync(textChannel.Name, textChannel.Id, textChannel.GuildId, cancellationToken)
                : Task.CompletedTask;

        /// <inheritdoc />
        public async Task HandleNotificationAsync(GuildAvailableNotification notification, CancellationToken cancellationToken = default)
        {
            foreach (var channel in notification.Guild.Channels.Where(x => x is ITextChannel))
                await _channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task HandleNotificationAsync(JoinedGuildNotification notification, CancellationToken cancellationToken = default)
        {
            if(notification.Guild.Available)
                foreach (var channel in notification.Guild.Channels.Where(x => x is ITextChannel))
                    await _channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId, cancellationToken);
        }

        private readonly IChannelService _channelService;
    }
}
