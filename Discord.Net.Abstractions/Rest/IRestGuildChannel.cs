using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuildChannel" />
    public interface IRestGuildChannel : IRestChannel, IGuildChannel
    {
        /// <inheritdoc cref="RestGuildChannel.CreateInviteAsync(int?, int?, bool, bool, RequestOptions)" />
        new Task<IRestInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null);

        /// <inheritdoc cref="RestGuildChannel.GetInvitesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestInviteMetadata>> GetInvitesAsync(RequestOptions options = null);
    }

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
            => (RestGuildChannel as IGuildChannel).Guild;

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
        public async Task<IRestInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
            => (await RestGuildChannel.CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, options))
                .Abstract();

        /// <inheritdoc />
        Task<IInviteMetadata> IGuildChannel.CreateInviteAsync(int? maxAge, int? maxUses, bool isTemporary, bool isUnique, RequestOptions options)
            => (RestGuildChannel as IGuildChannel).CreateInviteAsync(maxAge, maxUses, isTemporary, isUnique, options);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestGuildChannel.DeleteAsync(options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => (await RestGuildChannel.GetInvitesAsync(options))
                .Select(RestInviteMetadataAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        Task<IReadOnlyCollection<IInviteMetadata>> IGuildChannel.GetInvitesAsync(RequestOptions options)
            => (RestGuildChannel as IGuildChannel).GetInvitesAsync(options);

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
        Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
            => (RestGuildChannel as IGuildChannel).GetUserAsync(id, mode, options);

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
            => (RestGuildChannel as IGuildChannel).GetUsersAsync(mode, options);

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
            => (restGuildChannel is null) ? throw new ArgumentNullException(nameof(restGuildChannel))
                : (restGuildChannel is RestCategoryChannel restCategoryChannel) ? restCategoryChannel.Abstract()
                : (restGuildChannel is RestTextChannel restTextChannel) ? restTextChannel.Abstract()
                : (restGuildChannel is RestVoiceChannel restVoiceChannel) ? restVoiceChannel.Abstract()
                : new RestGuildChannelAbstraction(restGuildChannel) as IRestGuildChannel;
    }
}
