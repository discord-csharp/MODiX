using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketRole" />
    public interface ISocketRole : IRole
    {
        /// <inheritdoc cref="SocketRole.Guild" />
        new ISocketGuild Guild { get; }

        /// <inheritdoc cref="SocketRole.IsEveryone" />
        bool IsEveryone { get; }

        /// <inheritdoc cref="SocketRole.Members" />
        IEnumerable<ISocketGuildUser> Members { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketRole"/>, through the <see cref="ISocketRole"/> interface.
    /// </summary>
    internal class SocketRoleAbstraction : ISocketRole
    {
        /// <summary>
        /// Constructs a new <see cref="SocketRoleAbstraction"/> around an existing <see cref="WebSocket.SocketRole"/>.
        /// </summary>
        /// <param name="socketRole">The value to use for <see cref="WebSocket.SocketRole"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketRole"/>.</exception>
        public SocketRoleAbstraction(SocketRole socketRole)
        {
            SocketRole = socketRole ?? throw new ArgumentNullException(nameof(socketRole));
        }

        /// <inheritdoc />
        public Color Color
            => SocketRole.Color;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => SocketRole.CreatedAt;

        /// <inheritdoc />
        public ISocketGuild Guild
            => SocketRole.Guild
                .Abstract();

        /// <inheritdoc />
        IGuild IRole.Guild
            => (SocketRole as IRole).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong Id
            => SocketRole.Id;

        /// <inheritdoc />
        public bool IsEveryone
            => SocketRole.IsEveryone;

        /// <inheritdoc />
        public bool IsHoisted
            => SocketRole.IsHoisted;

        /// <inheritdoc />
        public bool IsManaged
            => SocketRole.IsManaged;

        /// <inheritdoc />
        public bool IsMentionable
            => SocketRole.IsMentionable;

        /// <inheritdoc />
        public IEnumerable<ISocketGuildUser> Members
            => SocketRole.Members
                .Select(SocketGuildUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public string Mention
            => SocketRole.Mention;

        /// <inheritdoc />
        public string Name
            => SocketRole.Name;

        /// <inheritdoc />
        public GuildPermissions Permissions
            => SocketRole.Permissions;

        /// <inheritdoc />
        public int Position
            => SocketRole.Position;

        /// <inheritdoc />
        public int CompareTo(IRole other)
            => SocketRole.CompareTo(other);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => SocketRole.DeleteAsync(options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null)
            => SocketRole.ModifyAsync(func, options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketRole"/> being abstracted.
        /// </summary>
        protected SocketRole SocketRole { get; }

        public RoleTags Tags
            => SocketRole.Tags;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketRole"/> objects.
    /// </summary>
    internal static class SocketRoleAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketRole"/> to an abstracted <see cref="ISocketRole"/> value.
        /// </summary>
        /// <param name="socketRole">The existing <see cref="SocketRole"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketRole"/>.</exception>
        /// <returns>An <see cref="ISocketRole"/> that abstracts <paramref name="socketRole"/>.</returns>
        public static ISocketRole Abstract(this SocketRole socketRole)
            => new SocketRoleAbstraction(socketRole);
    }
}
