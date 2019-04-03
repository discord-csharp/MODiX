using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Messaging
{
    /// <summary>
    /// Listens for events from <see cref="DiscordSocketClient"/> and dispatches them to the rest of the application,
    /// through an <see cref="IMessagePublisher"/>.
    /// </summary>
    public class DiscordSocketListeningBehavior : IBehavior
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordSocketListeningBehavior"/> from the given dependencies.
        /// </summary>
        public DiscordSocketListeningBehavior(IDiscordSocketClient discordSocketClient, IMessageDispatcher messageDispatcher)
        {
            DiscordSocketClient = discordSocketClient;
            MessageDispatcher = messageDispatcher;
        }

        /// <inheritdoc />
        public Task StartAsync()
        {
            DiscordSocketClient.MessageDeleted += OnMessageDeletedAsync;
            DiscordSocketClient.MessageReceived += OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated += OnMessageUpdatedAsync;
            DiscordSocketClient.ReactionAdded += OnReactionAddedAsync;
            DiscordSocketClient.ReactionRemoved += OnReactionRemovedAsync;
            DiscordSocketClient.UserBanned += OnUserBannedAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync()
        {
            DiscordSocketClient.MessageDeleted -= OnMessageDeletedAsync;
            DiscordSocketClient.MessageReceived -= OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated -= OnMessageUpdatedAsync;
            DiscordSocketClient.ReactionAdded -= OnReactionAddedAsync;
            DiscordSocketClient.ReactionRemoved -= OnReactionRemovedAsync;
            DiscordSocketClient.UserBanned -= OnUserBannedAsync;

            return Task.CompletedTask;
        }

        /// <summary>
        /// The <see cref="DiscordSocketClient"/> to be listened to.
        /// </summary>
        internal protected IDiscordSocketClient DiscordSocketClient { get; }

        /// <summary>
        /// A <see cref="IMessageDispatcher"/> used to dispatch discord notifications to the rest of the application.
        /// </summary>
        internal protected IMessageDispatcher MessageDispatcher { get; }

        private Task OnMessageDeletedAsync(ICacheable<IMessage, ulong> message, IISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageDeletedNotification(message, channel));

            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsync(ISocketMessage message)
        {
            MessageDispatcher.Dispatch(new MessageReceivedNotification(message));

            return Task.CompletedTask;
        }

        private Task OnMessageUpdatedAsync(ICacheable<IMessage, ulong> oldMessage, ISocketMessage newMessage, IISocketMessageChannel channel)
        {
            MessageDispatcher.Dispatch(new MessageUpdatedNotification(oldMessage, newMessage, channel));

            return Task.CompletedTask;
        }

        private Task OnReactionAddedAsync(ICacheable<IUserMessage, ulong> message, IISocketMessageChannel channel, ISocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionAddedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnReactionRemovedAsync(ICacheable<IUserMessage, ulong> message, IISocketMessageChannel channel, ISocketReaction reaction)
        {
            MessageDispatcher.Dispatch(new ReactionRemovedNotification(message, channel, reaction));

            return Task.CompletedTask;
        }

        private Task OnUserBannedAsync(ISocketUser user, ISocketGuild guild)
        {
            MessageDispatcher.Dispatch(new UserBannedNotification(user, guild));

            return Task.CompletedTask;
        }
    }
}
