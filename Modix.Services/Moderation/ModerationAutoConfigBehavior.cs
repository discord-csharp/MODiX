using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a behavior that automatically performs configuration necessary for an <see cref="IModerationService"/> to work.
    /// </summary>
    public class ModerationAutoConfigBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to DiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="ModerationAutoConfigBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        public ModerationAutoConfigBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordClient.ChannelCreated += OnChannelCreated;
            DiscordClient.ChannelUpdated += OnChannelUpdated;
            DiscordClient.LeftGuild += OnLeftGuild;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.GuildAvailable -= OnGuildAvailableAsync;
            DiscordClient.ChannelCreated -= OnChannelCreated;
            DiscordClient.ChannelUpdated -= OnChannelUpdated;
            DiscordClient.LeftGuild -= OnLeftGuild;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsRunning)
                OnStoppedAsync();

            base.Dispose(disposeManaged);
        }

        // TODO: Abstract DiscordSocketClient to DiscordSocketClient, or something, to make this testable
        /// <summary>
        /// A <see cref="DiscordSocketClient"/> for interacting with, and receiving events from, the Discord API.
        /// </summary>
        internal protected DiscordSocketClient DiscordClient { get; }

        private Task OnGuildAvailableAsync(IGuild guild)
            => SelfExecuteRequest<IModerationService>(x => x.AutoConfigureGuildAsync(guild));

        private Task OnChannelCreated(IChannel channel)
            => SelfExecuteRequest<IModerationService>(x => x.AutoConfigureChannelAsync(channel));

        private Task OnChannelUpdated(IChannel oldChannel, IChannel newChannel)
            => SelfExecuteRequest<IModerationService>(x => x.AutoConfigureChannelAsync(newChannel));

        private Task OnLeftGuild(IGuild guild)
            => SelfExecuteRequest<IModerationService>(x => x.UnConfigureGuildAsync(guild));
    }
}
