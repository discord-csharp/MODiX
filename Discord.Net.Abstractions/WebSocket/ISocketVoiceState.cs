using System;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketVoiceState" />
    public interface ISocketVoiceState : IVoiceState
    {
        /// <inheritdoc cref="SocketVoiceState.VoiceChannel" />
        new ISocketVoiceChannel VoiceChannel { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="SocketVoiceState"/>, through the <see cref="ISocketVoiceState"/> interface.
    /// </summary>
    internal struct SocketVoiceStateAbstraction : ISocketVoiceState
    {
        /// <summary>
        /// Constructs a new <see cref="SocketVoiceStateAbstraction"/> around an existing <see cref="SocketVoiceState"/>.
        /// </summary>
        /// <param name="socketVoiceState">The value to use for <see cref="SocketVoiceState"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceState"/>.</exception>
        public SocketVoiceStateAbstraction(SocketVoiceState socketVoiceState)
        {
            _socketVoiceState = socketVoiceState;
        }

        /// <inheritdoc />
        public ISocketVoiceChannel VoiceChannel
            => _socketVoiceState.VoiceChannel
                .Abstract();

        /// <inheritdoc />
        IVoiceChannel IVoiceState.VoiceChannel
            => _socketVoiceState.VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => _socketVoiceState.VoiceSessionId;

        /// <inheritdoc />
        public bool IsMuted
            => _socketVoiceState.IsMuted;

        /// <inheritdoc />
        public bool IsDeafened
            => _socketVoiceState.IsDeafened;

        /// <inheritdoc />
        public bool IsSuppressed
            => _socketVoiceState.IsSuppressed;

        /// <inheritdoc />
        public bool IsSelfMuted
            => _socketVoiceState.IsSelfMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => _socketVoiceState.IsSelfDeafened;

        /// <inheritdoc cref="SocketVoiceState.ToString" />
        public override string ToString()
            => _socketVoiceState.ToString();

        private readonly SocketVoiceState _socketVoiceState;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketVoiceState"/> objects.
    /// </summary>
    internal static class SocketVoiceStateAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketVoiceState"/> to an abstracted <see cref="ISocketVoiceState"/> value.
        /// </summary>
        /// <param name="socketVoiceState">The existing <see cref="SocketVoiceState"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceState"/>.</exception>
        /// <returns>An <see cref="ISocketVoiceState"/> that abstracts <paramref name="socketVoiceState"/>.</returns>
        public static ISocketVoiceState Abstract(this SocketVoiceState socketVoiceState)
            => new SocketVoiceStateAbstraction(socketVoiceState);
    }
}
