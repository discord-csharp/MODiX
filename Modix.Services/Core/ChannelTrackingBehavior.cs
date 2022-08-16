using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;

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
            INotificationHandler<JoinedGuildNotification>,
            INotificationHandler<ThreadCreatedNotification>,
            INotificationHandler<ThreadUpdatedNotification>
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
        public async Task HandleNotificationAsync(ChannelCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            if (notification.Channel is ITextChannel textChannel)
            {
                await TrackChannelAsync(textChannel, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task HandleNotificationAsync(ChannelUpdatedNotification notification, CancellationToken cancellationToken = default)
        {
            if (notification.NewChannel is ITextChannel textChannel)
            {
                await TrackChannelAsync(textChannel, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task HandleNotificationAsync(GuildAvailableNotification notification, CancellationToken cancellationToken = default)
        {
            foreach (var channel in notification.Guild.Channels.OfType<ITextChannel>())
                await TrackChannelAsync(channel, cancellationToken);
        }

        /// <inheritdoc />
        public async Task HandleNotificationAsync(JoinedGuildNotification notification, CancellationToken cancellationToken = default)
        {
            if (((IGuild)notification.Guild).Available)
                foreach (var channel in notification.Guild.Channels.OfType<ITextChannel>())
                    await TrackChannelAsync(channel, cancellationToken);
        }

        public async Task HandleNotificationAsync(ThreadCreatedNotification notification, CancellationToken cancellationToken = default)
            => await TrackChannelAsync(notification.Thread, cancellationToken);

        public async Task HandleNotificationAsync(ThreadUpdatedNotification notification, CancellationToken cancellationToken = default)
            => await TrackChannelAsync(notification.NewThread, cancellationToken);

        private async Task TrackChannelAsync(ITextChannel channel, CancellationToken cancellationToken)
            => await _channelService.TrackChannelAsync(channel.Name, channel.Id, channel.GuildId, channel is IThreadChannel threadChannel ? threadChannel.CategoryId : null, cancellationToken);

        private readonly IChannelService _channelService;
    }
}
