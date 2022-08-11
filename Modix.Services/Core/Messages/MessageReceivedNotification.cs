using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.MessageReceived"/> is raised.
    /// </summary>
    public class MessageReceivedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="MessageReceivedNotification"/> from the given values.
        /// </summary>
        /// <param name="message">The value to use for <see cref="Message"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="message"/>.</exception>
        public MessageReceivedNotification(SocketMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// The message that was received.
        /// </summary>
        public SocketMessage Message { get; }
    }
}
