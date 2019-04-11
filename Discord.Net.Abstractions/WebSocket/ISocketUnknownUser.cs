using System;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketUnknownUser" />
    public interface ISocketUnknownUser : ISocketUser { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketUnknownUser"/>, through the <see cref="ISocketUnknownUser"/> interface.
    /// </summary>
    internal class SocketUnknownUserAbstraction : SocketUserAbstraction, ISocketUnknownUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketUnknownUserAbstraction"/> around an existing <see cref="WebSocket.SocketUnknownUser"/>.
        /// </summary>
        /// <param name="socketUnknownUser">The value to use for <see cref="WebSocket.SocketUnknownUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUnknownUser"/>.</exception>
        public SocketUnknownUserAbstraction(SocketUnknownUser socketUnknownUser)
            : base(socketUnknownUser) { }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketUnknownUser"/> objects.
    /// </summary>
    internal static class SocketUnknownUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketUnknownUser"/> to an abstracted <see cref="ISocketUnknownUser"/> value.
        /// </summary>
        /// <param name="socketUnknownUser">The existing <see cref="SocketUnknownUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUnknownUser"/>.</exception>
        /// <returns>An <see cref="ISocketUnknownUser"/> that abstracts <paramref name="socketUnknownUser"/>.</returns>
        public static ISocketUnknownUser Abstract(this SocketUnknownUser socketUnknownUser)
            => new SocketUnknownUserAbstraction(socketUnknownUser);
    }
}
