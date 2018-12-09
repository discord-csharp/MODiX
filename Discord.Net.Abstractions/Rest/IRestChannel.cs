using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestChannel" />
    public interface IRestChannel : IChannel, IUpdateable { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestChannel"/>, through the <see cref="IRestChannel"/> interface.
    /// </summary>
    public class RestChannelAbstraction : IRestChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestChannelAbstraction"/> around an existing <see cref="Rest.RestChannel"/>.
        /// </summary>
        /// <param name="restChannel">The value to use for <see cref="Rest.RestChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restChannel"/>.</exception>
        public RestChannelAbstraction(RestChannel restChannel)
        {
            if (restChannel is null)
                throw new ArgumentNullException(nameof(restChannel));

            RestChannel = restChannel;
        }

        /// <inheritdoc />
        public string Name
            => (RestChannel as IChannel).Name;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestChannel.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => RestChannel.Id;

        /// <inheritdoc />
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestChannel as IChannel).GetUserAsync(id, mode, options);

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestChannel as IChannel).GetUsersAsync(mode, options);

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestChannel.UpdateAsync(options);

        /// <summary>
        /// The existing <see cref="Rest.RestChannel"/> being abstracted.
        /// </summary>
        protected RestChannel RestChannel { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestChannel"/> objects.
    /// </summary>
    public static class RestChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestChannel"/> to an abstracted <see cref="IRestChannel"/> value.
        /// </summary>
        /// <param name="restChannel">The existing <see cref="RestChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restChannel"/>.</exception>
        /// <returns>An <see cref="IRestChannel"/> that abstracts <paramref name="restChannel"/>.</returns>
        public static IRestChannel Abstract(this RestChannel restChannel)
            => (restChannel is null) ? throw new ArgumentNullException(nameof(restChannel))
                : (restChannel is RestDMChannel restDMChannel) ? restDMChannel.Abstract()
                : (restChannel is RestGroupChannel restGroupChannel) ? restGroupChannel.Abstract()
                : (restChannel is RestGuildChannel restGuildChannel) ? restGuildChannel.Abstract()
                : new RestChannelAbstraction(restChannel) as IRestChannel;
    }
}
