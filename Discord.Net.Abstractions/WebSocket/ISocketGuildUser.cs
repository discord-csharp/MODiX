using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord.Audio;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketGuildUser" />
    public interface ISocketGuildUser : ISocketUser, IGuildUser, IVoiceState
    {
        /// <inheritdoc cref="SocketGuildUser.VoiceState" />
        ISocketVoiceState VoiceState { get; }

        /// <inheritdoc cref="SocketGuildUser.VoiceChannel" />
        new ISocketVoiceChannel VoiceChannel { get; }

        /// <inheritdoc cref="SocketGuildUser.Roles" />
        IReadOnlyCollection<ISocketRole> Roles { get; }

        /// <inheritdoc cref="SocketGuildUser.AudioStream" />
        AudioInStream AudioStream { get; }

        /// <inheritdoc cref="SocketGuildUser.Hierarchy" />
        int Hierarchy { get; }

        /// <inheritdoc cref="SocketGuildUser.Guild" />
        new ISocketGuild Guild { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketGuildUser"/>, through the <see cref="ISocketGuildUser"/> interface.
    /// </summary>
    internal class SocketGuildUserAbstraction : SocketUserAbstraction, ISocketGuildUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketGuildUserAbstraction"/> around an existing <see cref="WebSocket.SocketGuildUser"/>.
        /// </summary>
        /// <param name="socketGuildUser">The value to use for <see cref="WebSocket.SocketGuildUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGuildUser"/>.</exception>
        public SocketGuildUserAbstraction(SocketGuildUser socketGuildUser)
            : base(socketGuildUser) { }

        /// <inheritdoc />
        public AudioInStream AudioStream
            => SocketGuildUser.AudioStream;

        /// <inheritdoc />
        public ISocketGuild Guild
            => SocketGuildUser.Guild
                .Abstract();

        /// <inheritdoc />
        IGuild IGuildUser.Guild
            => (SocketGuildUser as IGuildUser).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => (SocketGuildUser as IGuildUser).GuildId;

        /// <inheritdoc />
        public GuildPermissions GuildPermissions
            => SocketGuildUser.GuildPermissions;

        /// <inheritdoc />
        public int Hierarchy
            => SocketGuildUser.Hierarchy;

        /// <inheritdoc />
        public bool IsDeafened
            => SocketGuildUser.IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => SocketGuildUser.IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => SocketGuildUser.IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => SocketGuildUser.IsSelfMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => SocketGuildUser.IsSuppressed;

        /// <inheritdoc />
        public DateTimeOffset? JoinedAt
            => SocketGuildUser.JoinedAt;

        /// <inheritdoc />
        public string Nickname
            => SocketGuildUser.Nickname;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> RoleIds
            => (SocketGuildUser as IGuildUser).RoleIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketRole> Roles
            => SocketGuildUser.Roles
                .Select(SocketRoleAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public ISocketVoiceChannel VoiceChannel
            => SocketGuildUser.VoiceChannel
                .Abstract();

        /// <inheritdoc />
        IVoiceChannel IVoiceState.VoiceChannel
            => (SocketGuildUser as IVoiceState).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => SocketGuildUser.VoiceSessionId;

        /// <inheritdoc />
        public ISocketVoiceState VoiceState
            => SocketGuildUser.VoiceState
                ?.Abstract();

        /// <inheritdoc />
        public Task AddRoleAsync(IRole role, RequestOptions options = null)
            => SocketGuildUser.AddRoleAsync(role, options);

        /// <inheritdoc />
        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => SocketGuildUser.AddRolesAsync(roles, options);

        /// <inheritdoc />
        public ChannelPermissions GetPermissions(IGuildChannel channel)
            => SocketGuildUser.GetPermissions(channel);

        /// <inheritdoc />
        public Task KickAsync(string reason = null, RequestOptions options = null)
            => SocketGuildUser.KickAsync(reason, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
            => SocketGuildUser.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
            => SocketGuildUser.RemoveRoleAsync(role, options);

        /// <inheritdoc />
        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => SocketGuildUser.RemoveRolesAsync(roles, options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketGuildUser"/> being abstracted.
        /// </summary>
        protected SocketGuildUser SocketGuildUser
            => SocketUser as SocketGuildUser;

        public DateTimeOffset? PremiumSince => throw new NotImplementedException();

        public bool IsStreaming => throw new NotImplementedException();
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketGuildUser"/> objects.
    /// </summary>
    internal static class SocketGuildUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketGuildUser"/> to an abstracted <see cref="ISocketGuildUser"/> value.
        /// </summary>
        /// <param name="socketGuildUser">The existing <see cref="SocketGuildUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketGuildUser"/>.</exception>
        /// <returns>An <see cref="ISocketGuildUser"/> that abstracts <paramref name="socketGuildUser"/>.</returns>
        public static ISocketGuildUser Abstract(this SocketGuildUser socketGuildUser)
            => new SocketGuildUserAbstraction(socketGuildUser);
    }
}
