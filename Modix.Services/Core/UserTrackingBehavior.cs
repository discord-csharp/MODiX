using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Core
{
    /// <summary>
    /// Implements a behavior for keeping the data within an <see cref="IUserRepository"/> synchronized with Discord.NET.
    /// </summary>
    public class UserTrackingBehavior : BehaviorBase
    {
        // TODO: Abstract DiscordSocketClient to IDiscordSocketClient, or something, to make this testable
        /// <summary>
        /// Constructs a new <see cref="UserTrackingBehavior"/> object, with the given injected dependencies.
        /// See <see cref="BehaviorBase"/> for more details.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="serviceProvider">See <see cref="BehaviorBase"/>.</param>
        public UserTrackingBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.GuildAvailable += OnGuildAvailableAsync;
            DiscordClient.UserJoined += OnUserJoinedAsync;
            DiscordClient.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            DiscordClient.MessageReceived += OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.GuildAvailable -= OnGuildAvailableAsync;
            DiscordClient.UserJoined -= OnUserJoinedAsync;
            DiscordClient.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
            DiscordClient.MessageReceived -= OnMessageReceivedAsync;

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

        private Task OnGuildAvailableAsync(IGuild guild)
            => SelfExecuteOnScopedServiceAsync<IUserService>(async x =>
                await x.TrackUserAsync(
                    await guild.GetUserAsync(DiscordClient.CurrentUser.Id)));

        private Task OnUserJoinedAsync(IGuildUser guildUser)
            => SelfExecuteOnScopedServiceAsync<IUserService>(x => x.TrackUserAsync(guildUser));

        private Task OnGuildMemberUpdatedAsync(IGuildUser oldUser, IGuildUser newUser)
            => SelfExecuteOnScopedServiceAsync<IUserService>(x => x.TrackUserAsync(newUser));

        private Task OnMessageReceivedAsync(IMessage message)
            => SelfExecuteOnScopedServiceAsync<IUserService>(async x =>
            {
                if(message.Author is IGuildUser author)
                    await x.TrackUserAsync(author);
            });
    }
}
