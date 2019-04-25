using System;

using Discord;
using Discord.Commands;

namespace Modix.Bot
{
    /// <summary>
    /// Contains factory methods for assembling <see cref="AnyGuildMessage{TMessage}"/> values.
    /// </summary>
    public static class AnyGuildMessage
    {
        /// <summary>
        /// Constructs a new <see cref="AnyGuildMessage{TMessage}"/> value, from a given <see cref="IMessage"/> object
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> object to use for <see cref="AnyGuildMessage{TMessage}.Value"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="message"/>.</exception>
        /// <returns>A <see cref="AnyGuildMessage"/> value containing the given <see cref="IMessage"/> object.</returns>
        public static AnyGuildMessage<TMessage> FromMessage<TMessage>(TMessage message)
                where TMessage : IMessage
            => new AnyGuildMessage<TMessage>(message);
    }

    /// <summary>
    /// Represents a Discord command parameter that may be any message within any channel
    /// of the guild from which the command was issued. This differs from the default behavior
    /// for parameters of type <see cref="IMessage"/> or subtypes, where only cached messages
    /// within the same channel as the command are considered.
    /// </summary>
    public struct AnyGuildMessage<TMessage>
        where TMessage : IMessage
    {
        internal AnyGuildMessage(TMessage message)
        {
            Value = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// The message that was specified by the user, to be used as the command parameter.
        /// </summary>
        public TMessage Value { get; }
    }
}
