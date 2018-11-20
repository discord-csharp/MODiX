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
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
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
            DiscordClient.ChannelCreated += OnChannelCreatedAsync;
            DiscordClient.ChannelUpdated += OnChannelUpdatedAsync;
            DiscordClient.LeftGuild += OnLeftGuildAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.GuildAvailable -= OnGuildAvailableAsync;
            DiscordClient.ChannelCreated -= OnChannelCreatedAsync;
            DiscordClient.ChannelUpdated -= OnChannelUpdatedAsync;
            DiscordClient.LeftGuild -= OnLeftGuildAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override void Dispose(bool disposeManaged)
        {
            if (disposeManaged && IsRunning)
                OnStoppedAsync();

            base.Dispose(disposeManaged);
        }

        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// A <see cref="DiscordSocketClient"/> for interacting with, and receiving events from, the Discord API.
        /// </summary>
        internal protected DiscordSocketClient DiscordClient { get; }

        private async Task OnGuildAvailableAsync(IGuild guild)
            => await SelfExecuteRequest<IModerationService>(x => x.AutoConfigureGuildAsync(guild, DiscordClient.CurrentUser.Id));

        private async Task OnChannelCreatedAsync(IChannel channel)
            => await SelfExecuteRequest<IModerationService>(x => x.AutoConfigureChannelAsync(channel, DiscordClient.CurrentUser.Id));

        private async Task OnChannelUpdatedAsync(IChannel oldChannel, IChannel newChannel)
            => await SelfExecuteRequest<IModerationService>(x => x.AutoConfigureChannelAsync(newChannel, DiscordClient.CurrentUser.Id));

        private async Task OnLeftGuildAsync(IGuild guild)
            => await SelfExecuteRequest<IModerationService>(x => x.UnConfigureGuildAsync(guild, DiscordClient.CurrentUser.Id));
    }
}
