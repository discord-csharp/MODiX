using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketSystemMessage" />
    public interface ISocketSystemMessage : ISocketMessage, ISystemMessage { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketSystemMessage"/>, through the <see cref="ISocketSystemMessage"/> interface.
    /// </summary>
    public class SocketSystemMessageAbstraction : SocketMessageAbstraction, ISocketSystemMessage
    {
        /// <summary>
        /// Constructs a new <see cref="SocketSystemMessageAbstraction"/> around an existing <see cref="WebSocket.SocketSystemMessage"/>.
        /// </summary>
        /// <param name="socketSystemMessage">The value to use for <see cref="WebSocket.SocketSystemMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketSystemMessage"/>.</exception>
        public SocketSystemMessageAbstraction(SocketSystemMessage socketSystemMessage)
            : base(socketSystemMessage) { }

        /// <summary>
        /// The existing <see cref="WebSocket.SocketSystemMessage"/> being abstracted.
        /// </summary>
        protected SocketSystemMessage SocketSystemMessage
            => SocketMessage as SocketSystemMessage;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketSystemMessage"/> objects.
    /// </summary>
    public static class SocketSystemMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketSystemMessage"/> to an abstracted <see cref="ISocketSystemMessage"/> value.
        /// </summary>
        /// <param name="socketSystemMessage">The existing <see cref="SocketSystemMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketSystemMessage"/>.</exception>
        /// <returns>An <see cref="ISocketSystemMessage"/> that abstracts <paramref name="socketSystemMessage"/>.</returns>
        public static ISocketSystemMessage Abstract(this SocketSystemMessage socketSystemMessage)
            => new SocketSystemMessageAbstraction(socketSystemMessage);
    }
}
