using System;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketGroupUser" />
    public interface ISocketGroupUser : ISocketUser, IGroupUser
    {
        /// <inheritdoc cref="SocketGroupUser.Channel" />
        ISocketGroupChannel Channel { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketGroupUser"/>, through the <see cref="ISocketGroupUser"/> interface.
    /// </summary>
    internal class SocketGroupUserAbstraction : SocketUserAbstraction, ISocketGroupUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketGroupUserAbstraction"/> around an existing <see cref="WebSocket.SocketGroupUser"/>.
        /// </summary>
        /// <param name="socketGroupUser">The value to use for <see cref="WebSocket.SocketGroupUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGroupUser"/>.</exception>
        public SocketGroupUserAbstraction(SocketGroupUser socketGroupUser)
            : base(socketGroupUser) { }

        /// <inheritdoc />
        public ISocketGroupChannel Channel
            => SocketGroupUser.Channel
                .Abstract();

        /// <inheritdoc />
        public bool IsDeafened
            => (SocketGroupUser as IGroupUser).IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => (SocketGroupUser as IGroupUser).IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => (SocketGroupUser as IGroupUser).IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => (SocketGroupUser as IGroupUser).IsSelfMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => (SocketGroupUser as IGroupUser).IsSuppressed;

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel
            => (SocketGroupUser as IGroupUser).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => (SocketGroupUser as IGroupUser).VoiceSessionId;

        public bool IsStreaming => throw new NotImplementedException();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketGroupUser"/> being abstracted.
        /// </summary>
        protected SocketGroupUser SocketGroupUser
            => SocketUser as SocketGroupUser;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketGroupUser"/> objects.
    /// </summary>
    internal static class SocketGroupUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketGroupUser"/> to an abstracted <see cref="ISocketGroupUser"/> value.
        /// </summary>
        /// <param name="socketGroupUser">The existing <see cref="SocketGroupUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGroupUser"/>.</exception>
        /// <returns>An <see cref="ISocketGroupUser"/> that abstracts <paramref name="socketGroupUser"/>.</returns>
        public static ISocketGroupUser Abstract(this SocketGroupUser socketGroupUser)
            => new SocketGroupUserAbstraction(socketGroupUser);
    }
}
