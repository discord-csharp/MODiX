using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketChannel" />
    public interface ISocketChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
    {
        /// <inheritdoc cref="SocketChannel.Users" />
        IReadOnlyCollection<ISocketUser> Users { get; }

        /// <inheritdoc cref="SocketChannel.GetUser(ulong)" />
        ISocketUser GetUser(ulong id);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketChannel"/>, through the <see cref="ISocketChannel"/> interface.
    /// </summary>
    internal abstract class SocketChannelAbstraction : ISocketChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketChannelAbstraction"/> around an existing <see cref="WebSocket.SocketChannel"/>.
        /// </summary>
        /// <param name="socketChannel">The value to use for <see cref="WebSocket.SocketChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketChannel"/>.</exception>
        public SocketChannelAbstraction(SocketChannel socketChannel)
        {
            SocketChannel = socketChannel ?? throw new ArgumentNullException(nameof(socketChannel));
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => SocketChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => SocketChannel.Id;

        /// <inheritdoc />
        public string Name
            => (SocketChannel as IChannel).Name;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketUser> Users
            => SocketChannel.Users
                .Select(SocketUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public ISocketUser GetUser(ulong id)
            => SocketChannel.GetUser(id)
                .Abstract();

        /// <inheritdoc />
        public async Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (SocketChannel as IChannel).GetUserAsync(id, mode, options))
                .Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (SocketChannel as IChannel).GetUsersAsync(mode, options)
                .Select(x => x
                    .Select(UserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <summary>
        /// The existing <see cref="WebSocket.SocketChannel"/> being abstracted.
        /// </summary>
        protected SocketChannel SocketChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketChannel"/> objects.
    /// </summary>
    internal static class SocketChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketChannel"/> to an abstracted <see cref="ISocketChannel"/> value.
        /// </summary>
        /// <param name="socketChannel">The existing <see cref="SocketChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketChannel"/>.</exception>
        /// <returns>An <see cref="ISocketChannel"/> that abstracts <paramref name="socketChannel"/>.</returns>
        public static ISocketChannel Abstract(this SocketChannel socketChannel)
            => socketChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(socketChannel)),
                SocketDMChannel socketDMChannel
                    => socketDMChannel.Abstract() as ISocketChannel,
                SocketGroupChannel socketGroupChannel
                    => socketGroupChannel.Abstract() as ISocketChannel,
                SocketGuildChannel socketGuildChannel
                    => socketGuildChannel.Abstract() as ISocketChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(SocketChannel)} type {socketChannel.GetType().Name}")
            };
    }
}
