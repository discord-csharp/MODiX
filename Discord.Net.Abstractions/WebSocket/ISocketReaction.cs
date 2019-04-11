using System;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketReaction" />
    public interface ISocketReaction : IReaction
    {
        /// <inheritdoc cref="SocketReaction.UserId" />
        ulong UserId { get; }

        /// <inheritdoc cref="SocketReaction.User" />
        Optional<IUser> User { get; }

        /// <inheritdoc cref="SocketReaction.MessageId" />
        ulong MessageId { get; }

        /// <inheritdoc cref="SocketReaction.Message" />
        Optional<ISocketUserMessage> Message { get; }

        /// <inheritdoc cref="SocketReaction.Channel" />
        IISocketMessageChannel Channel { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketReaction"/>, through the <see cref="ISocketReaction"/> interface.
    /// </summary>
    internal class SocketReactionAbstraction : ISocketReaction
    {
        /// <summary>
        /// Constructs a new <see cref="SocketReactionAbstraction"/> around an existing <see cref="WebSocket.SocketReaction"/>.
        /// </summary>
        /// <param name="socketReaction">The value to use for <see cref="WebSocket.SocketReaction"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketReaction"/>.</exception>
        public SocketReactionAbstraction(SocketReaction socketReaction)
        {
            SocketReaction = socketReaction ?? throw new ArgumentNullException(nameof(socketReaction));
        }

        /// <inheritdoc />
        public IISocketMessageChannel Channel
            => SocketReaction.Channel
                .Abstract();

        /// <inheritdoc />
        public IEmote Emote
            => SocketReaction.Emote
                .Abstract();

        /// <inheritdoc />
        public ulong MessageId
            => SocketReaction.MessageId;

        /// <inheritdoc />
        public Optional<ISocketUserMessage> Message
            => SocketReaction.Message.IsSpecified
                ? new Optional<ISocketUserMessage>(SocketReaction.Message.Value
                    .Abstract())
                : Optional<ISocketUserMessage>.Unspecified;

        /// <inheritdoc />
        public Optional<IUser> User
            => SocketReaction.User.IsSpecified
                ? new Optional<IUser>(SocketReaction.User.Value
                    .Abstract())
                : Optional<IUser>.Unspecified;

        /// <inheritdoc />
        public ulong UserId
            => SocketReaction.UserId;

        /// <inheritdoc cref="SocketReaction.Equals(object)" />
        public override bool Equals(object other)
            => SocketReaction.Equals(other);

        /// <inheritdoc cref="SocketReaction.GetHashCode" />
        public override int GetHashCode()
            => SocketReaction.GetHashCode();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketReaction"/> being abstracted.
        /// </summary>
        protected SocketReaction SocketReaction { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketReaction"/> objects.
    /// </summary>
    internal static class SocketReactionAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketReaction"/> to an abstracted <see cref="ISocketReaction"/> value.
        /// </summary>
        /// <param name="socketReaction">The existing <see cref="SocketReaction"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketReaction"/>.</exception>
        /// <returns>An <see cref="ISocketReaction"/> that abstracts <paramref name="socketReaction"/>.</returns>
        public static ISocketReaction Abstract(this SocketReaction socketReaction)
            => new SocketReactionAbstraction(socketReaction);
    }
}
