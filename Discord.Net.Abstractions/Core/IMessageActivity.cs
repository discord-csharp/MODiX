using System;

namespace Discord
{
    /// <inheritdoc cref="MessageActivity" />
    public interface IMessageActivity
    {
        /// <inheritdoc cref="MessageActivity.Type" />
        MessageActivityType Type { get; }

        /// <inheritdoc cref="MessageActivity.PartyId" />
        string PartyId { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.MessageActivity"/>, through the <see cref="IMessageActivity"/> interface.
    /// </summary>
    public class MessageActivityAbstraction : IMessageActivity
    {
        /// <summary>
        /// Constructs a new <see cref="MessageActivityAbstraction"/> around an existing <see cref="Discord.MessageActivity"/>.
        /// </summary>
        /// <param name="messageActivity">The existing <see cref="Discord.MessageActivity"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="messageActivity"/>.</exception>
        public MessageActivityAbstraction(MessageActivity messageActivity)
        {
            MessageActivity = messageActivity ?? throw new ArgumentNullException(nameof(messageActivity));
        }

        /// <inheritdoc />
        public MessageActivityType Type
            => MessageActivity.Type;

        /// <inheritdoc />
        public string PartyId
            => MessageActivity.PartyId;

        /// <inheritdoc cref="MessageActivity.ToString()" />
        public override string ToString()
            => MessageActivity.ToString();

        /// <summary>
        /// The existing <see cref="Discord.MessageActivity"/> being abstracted.
        /// </summary>
        protected MessageActivity MessageActivity { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="MessageActivity"/> objects.
    /// </summary>
    public static class MessageActivityAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="MessageActivity"/> to an abstracted <see cref="IMessageActivity"/> value.
        /// </summary>
        /// <param name="messageActivity">The existing <see cref="MessageActivity"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="messageActivity"/>.</exception>
        /// <returns>An <see cref="IMessageActivity"/> that abstracts <paramref name="messageActivity"/>.</returns>
        public static IMessageActivity Abstract(this MessageActivity messageActivity)
            => new MessageActivityAbstraction(messageActivity);
    }

}
