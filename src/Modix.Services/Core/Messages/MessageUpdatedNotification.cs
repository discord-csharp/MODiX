using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="BaseSocketClient.MessageUpdated"/> is raised.
    /// </summary>
    public class MessageUpdatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="MessageUpdatedNotification"/> from the given values.
        /// </summary>
        /// <param name="oldMessage">The value to use for <see cref="OldMessage"/>.</param>
        /// <param name="newMessage">The value to use for <see cref="NewMessage"/>.</param>
        /// <param name="channel">The value to use for <see cref="Channel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="oldMessage"/>, <paramref name="newMessage"/>, and <paramref name="channel"/>.</exception>
        public MessageUpdatedNotification(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            OldMessage = oldMessage;
            NewMessage = newMessage;
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// The old version of the updated message, if it happens to have been cached.
        /// </summary>
        public Cacheable<IMessage, ulong> OldMessage { get; set; }

        /// <summary>
        /// The new version of the updated message.
        /// </summary>
        public SocketMessage NewMessage { get; set; }

        /// <summary>
        /// The channel in which the message was posted.
        /// </summary>
        public ISocketMessageChannel Channel { get; set; }
    }
}
