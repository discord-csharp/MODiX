using System;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketVoiceServer" />
    public interface ISocketVoiceServer
    {
        /// <inheritdoc cref="SocketVoiceServer.Guild" />
        Cacheable<IGuild, ulong> Guild { get; }

        /// <inheritdoc cref="SocketVoiceServer.Endpoint" />
        string Endpoint { get; }

        /// <inheritdoc cref="SocketVoiceServer.Token" />
        string Token { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketVoiceServer"/>, through the <see cref="ISocketVoiceServer"/> interface.
    /// </summary>
    public class SocketVoiceServerAbstraction : ISocketVoiceServer
    {
        /// <summary>
        /// Constructs a new <see cref="SocketVoiceServerAbstraction"/> around an existing <see cref="WebSocket.SocketVoiceServer"/>.
        /// </summary>
        /// <param name="socketVoiceServer">The value to use for <see cref="WebSocket.SocketVoiceServer"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceServer"/>.</exception>
        public SocketVoiceServerAbstraction(SocketVoiceServer socketVoiceServer)
        {
            if (socketVoiceServer is null)
                throw new ArgumentNullException(nameof(socketVoiceServer));

            SocketVoiceServer = socketVoiceServer;
        }

        /// <inheritdoc />
        public string Endpoint
            => SocketVoiceServer.Endpoint;

        /// <inheritdoc />
        public Cacheable<IGuild, ulong> Guild
            => SocketVoiceServer.Guild;

        /// <inheritdoc />
        public string Token
            => SocketVoiceServer.Token;

        /// <summary>
        /// The existing <see cref="WebSocket.SocketVoiceServer"/> being abstracted.
        /// </summary>
        protected SocketVoiceServer SocketVoiceServer { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketVoiceServer"/> objects.
    /// </summary>
    public static class SocketVoiceServerAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketVoiceServer"/> to an abstracted <see cref="ISocketVoiceServer"/> value.
        /// </summary>
        /// <param name="socketVoiceServer">The existing <see cref="SocketVoiceServer"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketVoiceServer"/>.</exception>
        /// <returns>An <see cref="ISocketVoiceServer"/> that abstracts <paramref name="socketVoiceServer"/>.</returns>
        public static ISocketVoiceServer Abstract(this SocketVoiceServer socketVoiceServer)
            => new SocketVoiceServerAbstraction(socketVoiceServer);
    }
}
