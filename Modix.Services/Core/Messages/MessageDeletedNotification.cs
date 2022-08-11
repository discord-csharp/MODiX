using System;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.MessageDeleted"/> is raised.
    /// </summary>
    public class MessageDeletedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="MessageDeletedNotification"/> from the given values.
        /// </summary>
        /// <param name="message">The value to use for <see cref="Message"/>.</param>
        /// <param name="message">The value to use for <see cref="Channel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="message"/> and <paramref name="channel"/>.</exception>
        public MessageDeletedNotification(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            Message = message;
            Channel = channel;
        }

        /// <summary>
        /// A cache entry for the message that was deleted.
        /// </summary>
        public Cacheable<IMessage, ulong> Message { get; }

        /// <summary>
        /// The channel from which the message was deleted.
        /// </summary>
        public Cacheable<IMessageChannel, ulong> Channel { get; }
    }
}
