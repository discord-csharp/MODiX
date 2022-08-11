using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.ReactionAdded"/> is raised.
    /// </summary>
    public class ReactionAddedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="ReactionAddedNotification"/> object from the given data values.
        /// </summary>
        /// <param name="message">The value to use for <see cref="Message"/>.</param>
        /// <param name="channel">The value to use for <see cref="Channel"/>.</param>
        /// <param name="reaction">The value to use for <see cref="Reaction"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="channel"/> and <paramref name="reaction"/>.</exception>
        public ReactionAddedNotification(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Message = message;
            Channel = channel;
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        /// <summary>
        /// The message (if cached) to which a reaction was added.
        /// </summary>
        public Cacheable<IUserMessage, ulong> Message { get; }

        /// <summary>
        /// The channel in which a reaction was added to a message.
        /// </summary>
        public Cacheable<IMessageChannel, ulong> Channel { get; }

        /// <summary>
        /// The reaction that was added to a message.
        /// </summary>
        public SocketReaction Reaction { get; }
    }
}
