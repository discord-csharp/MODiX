using System;
using System.Threading.Tasks;

using Serilog;

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
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordClient"/>.</exception>
        public UserTrackingBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }

        /// <inheritdoc />
        internal protected override Task OnStartingAsync()
        {
            DiscordClient.Connected += OnConnectedAsync;
            DiscordClient.UserJoined += OnUserJoinedAsync;
            DiscordClient.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            DiscordClient.MessageReceived += OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.Connected -= OnConnectedAsync;
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

        private Task OnConnectedAsync()
            => SelfExecuteOnScopedServiceAsync<IUserService>(x => x.TrackUserAsync(DiscordClient.CurrentUser));

        private Task OnUserJoinedAsync(IGuildUser guildUser)
            => SelfExecuteOnScopedServiceAsync<IUserService>(x => x.TrackUserAsync(guildUser));

        private Task OnGuildMemberUpdatedAsync(IGuildUser oldUser, IGuildUser newUser)
            => SelfExecuteOnScopedServiceAsync<IUserService>(x =>
            {
                if(newUser.Username == null)
                    Log.Error($"OnGuildMemberUpdatedAsync:\r\n ~ newUser.Id: {newUser.Id}\r\n ~ newUser.Discriminator: {newUser.Discriminator ?? "null"}\r\n ~ newUser.Nickname: {newUser.Nickname ?? "null"}\r\n ~ newUser.IsBot: {newUser.IsBot}\r\n ~ newUser.IsWebhook: {newUser.IsWebhook}");

                return x.TrackUserAsync(newUser);
            });

        private Task OnMessageReceivedAsync(IMessage message)
            => SelfExecuteOnScopedServiceAsync<IUserService>(x =>
            {
                if (message.Author.Username == null)
                    Log.Error($"OnMessageReceivedAsync:\r\n ~ message.Source: {message.Source}\r\n ~ message.Type: {message.Type}\r\n ~ message.Content: {message.Content}\r\n ~ message.Author.Id: {message.Author.Id}\r\n ~ newUser.Discriminator: {message.Author.Discriminator ?? "null"}\r\n ~ newUser.IsBot: {newUser.IsBot}\r\n ~ newUser.IsWebhook: {newUser.IsWebhook}");

                return x.TrackUserAsync(message.Author);
            });
    }
}
