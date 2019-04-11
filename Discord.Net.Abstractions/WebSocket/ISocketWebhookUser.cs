using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketWebhookUser" />
    public interface ISocketWebhookUser : ISocketUser, IWebhookUser
    {
        /// <inheritdoc cref="SocketWebhookUser.Guild" />
        new ISocketGuild Guild { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketWebhookUser"/>, through the <see cref="ISocketWebhookUser"/> interface.
    /// </summary>
    internal class SocketWebhookUserAbstraction : SocketUserAbstraction, ISocketWebhookUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketWebhookUserAbstraction"/> around an existing <see cref="WebSocket.SocketWebhookUser"/>.
        /// </summary>
        /// <param name="socketWebhookUser">The value to use for <see cref="WebSocket.SocketWebhookUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketWebhookUser"/>.</exception>
        public SocketWebhookUserAbstraction(SocketWebhookUser socketWebhookUser)
            : base(socketWebhookUser) { }

        /// <inheritdoc />
        public ISocketGuild Guild
            => SocketWebhookUser.Guild
                .Abstract();

        /// <inheritdoc />
        IGuild IGuildUser.Guild
            => (SocketWebhookUser as IGuildUser).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => (SocketWebhookUser as IGuildUser).GuildId;

        /// <inheritdoc />
        public GuildPermissions GuildPermissions
            => (SocketWebhookUser as IGuildUser).GuildPermissions;

        /// <inheritdoc />
        public bool IsDeafened
            => (SocketWebhookUser as IVoiceState).IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => (SocketWebhookUser as IVoiceState).IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => (SocketWebhookUser as IVoiceState).IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => (SocketWebhookUser as IVoiceState).IsSelfMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => (SocketWebhookUser as IVoiceState).IsSelfMuted;

        /// <inheritdoc />
        public DateTimeOffset? JoinedAt
            => (SocketWebhookUser as IGuildUser).JoinedAt;

        /// <inheritdoc />
        public string Nickname
            => (SocketWebhookUser as IGuildUser).Nickname;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> RoleIds
            => (SocketWebhookUser as IGuildUser).RoleIds;

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel
            => (SocketWebhookUser as IVoiceState).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => (SocketWebhookUser as IVoiceState).VoiceSessionId;

        /// <inheritdoc />
        public ulong WebhookId
            => SocketWebhookUser.WebhookId;

        /// <inheritdoc />
        public Task AddRoleAsync(IRole role, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).AddRoleAsync(role, options);

        /// <inheritdoc />
        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).AddRolesAsync(roles, options);

        /// <inheritdoc />
        public ChannelPermissions GetPermissions(IGuildChannel channel)
            => (SocketWebhookUser as IGuildUser).GetPermissions(channel);

        /// <inheritdoc />
        public Task KickAsync(string reason = null, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).KickAsync(reason, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).ModifyAsync(func, options);

        /// <inheritdoc />
        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).RemoveRoleAsync(role, options);

        /// <inheritdoc />
        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => (SocketWebhookUser as IGuildUser).RemoveRolesAsync(roles, options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketWebhookUser"/> being abstracted.
        /// </summary>
        protected SocketWebhookUser SocketWebhookUser
            => SocketUser as SocketWebhookUser;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketWebhookUser"/> objects.
    /// </summary>
    internal static class SocketWebhookUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketWebhookUser"/> to an abstracted <see cref="ISocketWebhookUser"/> value.
        /// </summary>
        /// <param name="socketWebhookUser">The existing <see cref="SocketWebhookUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketWebhookUser"/>.</exception>
        /// <returns>An <see cref="ISocketWebhookUser"/> that abstracts <paramref name="socketWebhookUser"/>.</returns>
        public static ISocketWebhookUser Abstract(this SocketWebhookUser socketWebhookUser)
            => new SocketWebhookUserAbstraction(socketWebhookUser);
    }
}
