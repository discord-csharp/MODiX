using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuildChannel" />
    public interface IRestGuildChannel : IRestChannel, IGuildChannel { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGuildChannel"/>, through the <see cref="IRestGuildChannel"/> interface.
    /// </summary>
    public class RestGuildChannelAbstraction : RestChannelAbstraction, IRestGuildChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestGuildChannelAbstraction"/> around an existing <see cref="Rest.RestGuildChannel"/>.
        /// </summary>
        /// <param name="restGuildChannel">The value to use for <see cref="Rest.RestGuildChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildChannel"/>.</exception>
        public RestGuildChannelAbstraction(RestGuildChannel restGuildChannel)
            : base(restGuildChannel) { }

        /// <inheritdoc />
        public IGuild Guild
            => (RestGuildChannel as IGuildChannel).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => RestGuildChannel.GuildId;

        /// <inheritdoc />
        public IReadOnlyCollection<Overwrite> PermissionOverwrites
            => RestGuildChannel.PermissionOverwrites;

        /// <inheritdoc />
        public int Position
            => RestGuildChannel.Position;

        /// <inheritdoc />
        public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
            => RestGuildChannel.AddPermissionOverwriteAsync(role, permissions, options);

        /// <inheritdoc />
        public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null)
            => RestGuildChannel.AddPermissionOverwriteAsync(user, permissions, options);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestGuildChannel.DeleteAsync(options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
            => RestGuildChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public OverwritePermissions? GetPermissionOverwrite(IRole role)
            => RestGuildChannel.GetPermissionOverwrite(role);

        /// <inheritdoc />
        public OverwritePermissions? GetPermissionOverwrite(IUser user)
            => RestGuildChannel.GetPermissionOverwrite(user);

        /// <inheritdoc />
        async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
            => (await (RestGuildChannel as IGuildChannel).GetUserAsync(id, mode, options))
                .Abstract();

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
            => (RestGuildChannel as IGuildChannel).GetUsersAsync(mode, options)
                .Select(x => x
                    .Select(GuildUserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
            => RestGuildChannel.RemovePermissionOverwriteAsync(role, options);

        /// <inheritdoc />
        public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
            => RestGuildChannel.RemovePermissionOverwriteAsync(user, options);

        /// <inheritdoc cref="RestGuildChannel.ToString" />
        public override string ToString()
            => RestGuildChannel.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestGuildChannel"/> being abstracted.
        /// </summary>
        protected RestGuildChannel RestGuildChannel
            => RestChannel as RestGuildChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGuildChannel"/> objects.
    /// </summary>
    public static class RestGuildChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGuildChannel"/> to an abstracted <see cref="IRestGuildChannel"/> value.
        /// </summary>
        /// <param name="restGuildChannel">The existing <see cref="RestGuildChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildChannel"/>.</exception>
        /// <returns>An <see cref="IRestGuildChannel"/> that abstracts <paramref name="restGuildChannel"/>.</returns>
        public static IRestGuildChannel Abstract(this RestGuildChannel restGuildChannel)
            => restGuildChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(restGuildChannel)),
                RestCategoryChannel restCategoryChannel
                    => restCategoryChannel.Abstract(),
                RestTextChannel restTextChannel
                    => restTextChannel.Abstract(),
                RestVoiceChannel restVoiceChannel
                    => restVoiceChannel.Abstract(),
                _
                    => new RestGuildChannelAbstraction(restGuildChannel) as IRestGuildChannel
            };
    }
}
