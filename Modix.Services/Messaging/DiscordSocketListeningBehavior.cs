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
            DiscordSocketClient.MessageReceived += OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated += OnMessageUpdatedAsync;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync()
        {
            DiscordSocketClient.MessageReceived -= OnMessageReceivedAsync;
            DiscordSocketClient.MessageUpdated -= OnMessageUpdatedAsync;

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
    }
}
