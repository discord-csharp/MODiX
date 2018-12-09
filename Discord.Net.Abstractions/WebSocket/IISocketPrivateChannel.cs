using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="ISocketPrivateChannel" />
    public interface IISocketPrivateChannel : IPrivateChannel
    {
        /// <inheritdoc cref="ISocketPrivateChannel.Recipients" />
        new IReadOnlyCollection<ISocketUser> Recipients { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.ISocketPrivateChannel"/>, through the <see cref="IISocketPrivateChannel"/> interface.
    /// </summary>
    public class ISocketPrivateChannelAbstraction : IISocketPrivateChannel
    {
        /// <summary>
        /// Constructs a new <see cref="ISocketPrivateChannelAbstraction"/> around an existing <see cref="WebSocket.ISocketPrivateChannel"/>.
        /// </summary>
        /// <param name="iSocketPrivateChannel">The value to use for <see cref="WebSocket.ISocketPrivateChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketPrivateChannel"/>.</exception>
        public ISocketPrivateChannelAbstraction(ISocketPrivateChannel iSocketPrivateChannel)
        {
            if (iSocketPrivateChannel is null)
                throw new ArgumentNullException(nameof(iSocketPrivateChannel));

            ISocketPrivateChannel = iSocketPrivateChannel;
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => ISocketPrivateChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => ISocketPrivateChannel.Id;

        /// <inheritdoc />
        public string Name
            => ISocketPrivateChannel.Name;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketUser> Recipients
            => ISocketPrivateChannel.Recipients
                .Select(SocketUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (ISocketPrivateChannel as IPrivateChannel).Recipients;

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => ISocketPrivateChannel.GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => ISocketPrivateChannel.GetUsersAsync(mode, options);

        /// <summary>
        /// The existing <see cref="WebSocket.ISocketPrivateChannel"/> being abstracted.
        /// </summary>
        protected ISocketPrivateChannel ISocketPrivateChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="ISocketPrivateChannel"/> objects.
    /// </summary>
    public static class ISocketPrivateChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ISocketPrivateChannel"/> to an abstracted <see cref="IISocketPrivateChannel"/> value.
        /// </summary>
        /// <param name="iSocketPrivateChannel">The existing <see cref="ISocketPrivateChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketPrivateChannel"/>.</exception>
        /// <returns>An <see cref="IISocketPrivateChannel"/> that abstracts <paramref name="iSocketPrivateChannel"/>.</returns>
        public static IISocketPrivateChannel Abstract(this ISocketPrivateChannel iSocketPrivateChannel)
            => new ISocketPrivateChannelAbstraction(iSocketPrivateChannel);
    }
}
