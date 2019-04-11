using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketGuildChannel" />
    public interface ISocketGuildChannel : ISocketChannel, IGuildChannel, IDeletable
    {
        /// <inheritdoc cref="SocketGuildChannel.Users" />
        new IReadOnlyCollection<ISocketGuildUser> Users { get; }

        /// <inheritdoc cref="SocketGuildChannel.Guild" />
        new ISocketGuild Guild { get; }

        /// <inheritdoc cref="SocketGuildChannel.GetUser(ulong)" />
        new ISocketGuildUser GetUser(ulong id);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketGuildChannel"/>, through the <see cref="ISocketGuildChannel"/> interface.
    /// </summary>
    internal class SocketGuildChannelAbstraction : SocketChannelAbstraction, ISocketGuildChannel
    {
        /// <summary>
        /// Constructs a new <see cref="SocketGuildChannelAbstraction"/> around an existing <see cref="WebSocket.SocketGuildChannel"/>.
        /// </summary>
        /// <param name="socketGuildChannel">The value to use for <see cref="WebSocket.SocketGuildChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGuildChannel"/>.</exception>
        public SocketGuildChannelAbstraction(SocketGuildChannel socketGuildChannel)
            : base(socketGuildChannel) { }

        /// <inheritdoc />
        new public IReadOnlyCollection<ISocketGuildUser> Users
            => SocketGuildChannel.Users
                .Select(SocketGuildUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public ISocketGuild Guild
            => SocketGuildChannel.Guild
                .Abstract();

        /// <inheritdoc />
        IGuild IGuildChannel.Guild
            => (SocketGuildChannel as IGuildChannel).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => (SocketGuildChannel as IGuildChannel).GuildId;

        /// <inheritdoc />
        public IReadOnlyCollection<Overwrite> PermissionOverwrites
            => SocketGuildChannel.PermissionOverwrites;

        /// <inheritdoc />
        public int Position
            => SocketGuildChannel.Position;

        /// <inheritdoc />
        public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
            => SocketGuildChannel.AddPermissionOverwriteAsync(role, permissions, options);

        /// <inheritdoc />
        public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null)
            => SocketGuildChannel.AddPermissionOverwriteAsync(user, permissions, options);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => SocketGuildChannel.DeleteAsync(options);

        /// <inheritdoc />
        public OverwritePermissions? GetPermissionOverwrite(IRole role)
            => SocketGuildChannel.GetPermissionOverwrite(role);

        /// <inheritdoc />
        public OverwritePermissions? GetPermissionOverwrite(IUser user)
            => SocketGuildChannel.GetPermissionOverwrite(user);

        /// <inheritdoc />
        new public ISocketGuildUser GetUser(ulong id)
            => SocketGuildChannel.GetUser(id)
                .Abstract();

        /// <inheritdoc />
        async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
            => (await (SocketGuildChannel as IGuildChannel).GetUserAsync(id, mode, options))
                .Abstract();

        /// <inheritdoc />
        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
            => (SocketGuildChannel as IGuildChannel).GetUsersAsync(mode, options)
                .Select(x => x
                    .Select(GuildUserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
            => SocketGuildChannel.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
            => SocketGuildChannel.RemovePermissionOverwriteAsync(role, options);

        /// <inheritdoc />
        public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
            => SocketGuildChannel.RemovePermissionOverwriteAsync(user, options);

        /// <inheritdoc cref="SocketGuildChannel.ToString" />
        public override string ToString()
            => SocketGuildChannel.ToString();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketGuildChannel"/> being abstracted.
        /// </summary>
        protected SocketGuildChannel SocketGuildChannel
            => SocketChannel as SocketGuildChannel;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketGuildChannel"/> objects.
    /// </summary>
    internal static class SocketGuildChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketGuildChannel"/> to an abstracted <see cref="ISocketGuildChannel"/> value.
        /// </summary>
        /// <param name="socketGuildChannel">The existing <see cref="SocketGuildChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGuildChannel"/>.</exception>
        /// <returns>An <see cref="ISocketGuildChannel"/> that abstracts <paramref name="socketGuildChannel"/>.</returns>
        public static ISocketGuildChannel Abstract(this SocketGuildChannel socketGuildChannel)
            => socketGuildChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(socketGuildChannel)),
                SocketCategoryChannel socketCategoryChannel
                    => SocketCategoryChannelAbstractionExtensions.Abstract(socketCategoryChannel),
                SocketTextChannel socketTextChannel
                    => SocketTextChannelAbstractionExtensions.Abstract(socketTextChannel),
                SocketVoiceChannel socketVoiceChannel
                    => SocketVoiceChannelAbstractionExtensions.Abstract(socketVoiceChannel),
                _
                    => new SocketGuildChannelAbstraction(socketGuildChannel) as ISocketGuildChannel
            };
    }
}
