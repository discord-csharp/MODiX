using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuildUser" />
    public interface IRestGuildUser : IRestUser, IGuildUser, IVoiceState { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGuildUser"/>, through the <see cref="IRestGuildUser"/> interface.
    /// </summary>
    internal class RestGuildUserAbstraction : RestUserAbstraction, IRestGuildUser
    {
        /// <summary>
        /// Constructs a new <see cref="RestGuildUserAbstraction"/> around an existing <see cref="Rest.RestGuildUser"/>.
        /// </summary>
        /// <param name="restGuildUser">The value to use for <see cref="Rest.RestGuildUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildUser"/>.</exception>
        public RestGuildUserAbstraction(RestGuildUser restGuildUser)
            : base(restGuildUser) { }

        /// <inheritdoc />
        public IGuild Guild
            => (RestGuildUser as IGuildUser).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => RestGuildUser.GuildId;

        /// <inheritdoc />
        public GuildPermissions GuildPermissions
            => RestGuildUser.GuildPermissions;

        /// <inheritdoc />
        public bool IsDeafened
            => RestGuildUser.IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => RestGuildUser.IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => (RestGuildUser as IGuildUser).IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => (RestGuildUser as IGuildUser).IsSelfMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => (RestGuildUser as IGuildUser).IsSuppressed;

        /// <inheritdoc />
        public DateTimeOffset? JoinedAt
            => RestGuildUser.JoinedAt;

        /// <inheritdoc />
        public string Nickname
            => RestGuildUser.Nickname;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> RoleIds
            => RestGuildUser.RoleIds;

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel
            => (RestGuildUser as IVoiceState).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => (RestGuildUser as IVoiceState).VoiceSessionId;

        /// <inheritdoc />
        public Task AddRoleAsync(IRole role, RequestOptions options = null)
            => RestGuildUser.AddRoleAsync(role);

        /// <inheritdoc />
        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => RestGuildUser.AddRolesAsync(roles);

        /// <inheritdoc />
        public Task KickAsync(string reason = null, RequestOptions options = null)
            => RestGuildUser.KickAsync(reason, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
            => RestGuildUser.ModifyAsync(func, options);

        /// <inheritdoc />
        public ChannelPermissions GetPermissions(IGuildChannel channel)
            => RestGuildUser.GetPermissions(channel);

        /// <inheritdoc />
        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
            => RestGuildUser.RemoveRoleAsync(role, options);

        /// <inheritdoc />
        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => RestGuildUser.RemoveRolesAsync(roles, options);

        /// <summary>
        /// The existing <see cref="Rest.RestGuildUser"/> being abstracted.
        /// </summary>
        protected RestGuildUser RestGuildUser
            => RestUser as RestGuildUser;

        public DateTimeOffset? PremiumSince => throw new NotImplementedException();

        public bool IsStreaming => throw new NotImplementedException();
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGuildUser"/> objects.
    /// </summary>
    internal static class RestGuildUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGuildUser"/> to an abstracted <see cref="IRestGuildUser"/> value.
        /// </summary>
        /// <param name="restGuildUser">The existing <see cref="RestGuildUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuildUser"/>.</exception>
        /// <returns>An <see cref="IRestGuildUser"/> that abstracts <paramref name="restGuildUser"/>.</returns>
        public static IRestGuildUser Abstract(this RestGuildUser restGuildUser)
            => new RestGuildUserAbstraction(restGuildUser);
    }
}
