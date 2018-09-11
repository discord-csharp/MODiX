using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Core
{
    /// <summary>
    /// Implements a behavior for keeping the channel data within the local datastore synchronized with the Discord API.
    /// </summary>
    public class ChannelTrackingBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="ChannelTrackingBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        public ChannelTrackingBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.Ready += OnReady;
            DiscordClient.JoinedGuild += OnJoinedGuild;
            DiscordClient.ChannelCreated += OnChannelCreated;
            DiscordClient.ChannelUpdated += OnChannelUpdated;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.Ready -= OnReady;
            DiscordClient.JoinedGuild -= OnJoinedGuild;
            DiscordClient.ChannelCreated -= OnChannelCreated;
            DiscordClient.ChannelUpdated -= OnChannelUpdated;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsRunning)
                OnStoppedAsync();

            base.Dispose(disposeManaged);
        }

        /// <summary>
        /// A <see cref="DiscordSocketClient"/> to be used for interacting with the Discord API.
        /// </summary>
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnReady()
            => SelfExecuteRequest<IChannelService>(async channelService =>
            {
                foreach(var channel in DiscordClient.Guilds
                        .SelectMany(x => x.Channels)
                        .Where(x => x is IMessageChannel))
                    await channelService.TrackChannelAsync(channel);
            });

        private Task OnJoinedGuild(IGuild guild)
            => SelfExecuteRequest<IChannelService>(async channelService =>
            {
                foreach (var channel in (await guild.GetChannelsAsync())
                        .Where(x => x is IMessageChannel))
                    await channelService.TrackChannelAsync(channel);
            });

        private Task OnChannelCreated(IChannel channel)
            => (channel is IMessageChannel) && (channel is IGuildChannel guildChannel) && !(guildChannel.Guild is null)
                ? SelfExecuteRequest<IChannelService>(channelService => channelService.TrackChannelAsync(guildChannel))
                : Task.CompletedTask;

        private Task OnChannelUpdated(IChannel oldChannel, IChannel newChannel)
            => (newChannel is IMessageChannel) && (newChannel is IGuildChannel guildChannel) && !(guildChannel.Guild is null)
                ? SelfExecuteRequest<IChannelService>(channelService => channelService.TrackChannelAsync(guildChannel))
                : Task.CompletedTask;
    }
}
