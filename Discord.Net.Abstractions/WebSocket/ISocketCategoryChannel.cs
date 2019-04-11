using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketCategoryChannel" />
    public interface ISocketCategoryChannel : ISocketGuildChannel, ICategoryChannel
    {
        /// <inheritdoc cref="SocketCategoryChannel.Channels" />
        IReadOnlyCollection<ISocketGuildChannel> Channels { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketCategoryChannel"/>, through the <see cref="ISocketCategoryChannel"/> interface.
    /// </summary>
    internal class SocketCategoryChannelAbstraction : SocketGuildChannelAbstraction, ISocketCategoryChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketCategoryChannelAbstraction"/> around an existing <see cref="WebSocket.SocketCategoryChannel"/>.
        /// </summary>
        /// <param name="socketCategoryChannel">The value to use for <see cref="WebSocket.SocketCategoryChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketCategoryChannel"/>.</exception>
        public SocketCategoryChannelAbstraction(SocketCategoryChannel socketCategoryChannel)
            : base(socketCategoryChannel) { }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGuildChannel> Channels
            => SocketCategoryChannel.Channels
                .Select(SocketGuildChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketCategoryChannel"/> being abstracted.
        /// </summary>
        protected SocketCategoryChannel SocketCategoryChannel
            => SocketGuildChannel as SocketCategoryChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketCategoryChannel"/> objects.
    /// </summary>
    internal static class SocketCategoryChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketCategoryChannel"/> to an abstracted <see cref="ISocketCategoryChannel"/> value.
        /// </summary>
        /// <param name="socketCategoryChannel">The existing <see cref="SocketCategoryChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketCategoryChannel"/>.</exception>
        /// <returns>An <see cref="ISocketCategoryChannel"/> that abstracts <paramref name="socketCategoryChannel"/>.</returns>
        public static ISocketCategoryChannel Abstract(SocketCategoryChannel socketCategoryChannel)
            => new SocketCategoryChannelAbstraction(socketCategoryChannel);
    }
}
