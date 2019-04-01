using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="IRestPrivateChannel" />
    public interface IIRestPrivateChannel : IPrivateChannel
    {
        /// <inheritdoc cref="IIRestPrivateChannel" />
        new IReadOnlyCollection<IRestUser> Recipients { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.IRestPrivateChannel"/>, through the <see cref="IIRestPrivateChannel"/> interface.
    /// </summary>
    public class IRestPrivateChannelAbstraction : IIRestPrivateChannel
    {
        /// <summary>
        /// Constructs a new <see cref="IRestPrivateChannelAbstraction"/> around an existing <see cref="Rest.IRestPrivateChannel"/>.
        /// </summary>
        /// <param name="iRestPrivateChannel">The value to use for <see cref="Rest.IRestPrivateChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestPrivateChannel"/>.</exception>
        public IRestPrivateChannelAbstraction(IRestPrivateChannel iRestPrivateChannel)
        {
            IRestPrivateChannel = iRestPrivateChannel ?? throw new ArgumentNullException(nameof(iRestPrivateChannel));
        }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => IRestPrivateChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => IRestPrivateChannel.Id;

        /// <inheritdoc />
        public string Name
            => IRestPrivateChannel.Name;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestUser> Recipients
            => IRestPrivateChannel.Recipients
                .Select(RestUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IUser> IPrivateChannel.Recipients
            => (IRestPrivateChannel as IPrivateChannel).Recipients;

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestPrivateChannel.GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => IRestPrivateChannel.GetUsersAsync(mode, options);

        /// <summary>
        /// The existing <see cref="Rest.IRestPrivateChannel"/> being abstracted.
        /// </summary>
        protected IRestPrivateChannel IRestPrivateChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="IRestPrivateChannel"/> objects.
    /// </summary>
    public static class IRestPrivateChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IRestPrivateChannel"/> to an abstracted <see cref="IIRestPrivateChannel"/> value.
        /// </summary>
        /// <param name="iRestPrivateChannel">The existing <see cref="IRestPrivateChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestPrivateChannel"/>.</exception>
        /// <returns>An <see cref="IIRestPrivateChannel"/> that abstracts <paramref name="iRestPrivateChannel"/>.</returns>
        public static IIRestPrivateChannel Abstract(this IRestPrivateChannel iRestPrivateChannel)
            => new IRestPrivateChannelAbstraction(iRestPrivateChannel);
    }
}
