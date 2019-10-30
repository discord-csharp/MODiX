using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestWebhookUser" />
    public interface IRestWebhookUser : IRestUser, IWebhookUser
    {
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebRest.RestWebhookUser"/>, through the <see cref="IRestWebhookUser"/> interface.
    /// </summary>
    internal class RestWebhookUserAbstraction : RestUserAbstraction, IRestWebhookUser
    {
        /// <summary>
        /// Constructs a new <see cref="RestWebhookUserAbstraction"/> around an existing <see cref="WebRest.RestWebhookUser"/>.
        /// </summary>
        /// <param name="restWebhookUser">The value to use for <see cref="WebRest.RestWebhookUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restWebhookUser"/>.</exception>
        public RestWebhookUserAbstraction(RestWebhookUser restWebhookUser)
            : base(restWebhookUser) { }

        /// <inheritdoc />
        IGuild IGuildUser.Guild
            => (RestWebhookUser as IGuildUser).Guild
                ?.Abstract();

        /// <inheritdoc />
        public ulong GuildId
            => (RestWebhookUser as IGuildUser).GuildId;

        /// <inheritdoc />
        public GuildPermissions GuildPermissions
            => (RestWebhookUser as IGuildUser).GuildPermissions;

        /// <inheritdoc />
        public bool IsDeafened
            => (RestWebhookUser as IVoiceState).IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => (RestWebhookUser as IVoiceState).IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => (RestWebhookUser as IVoiceState).IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => (RestWebhookUser as IVoiceState).IsSelfMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => (RestWebhookUser as IVoiceState).IsSelfMuted;

        /// <inheritdoc />
        public DateTimeOffset? JoinedAt
            => (RestWebhookUser as IGuildUser).JoinedAt;

        /// <inheritdoc />
        public string Nickname
            => (RestWebhookUser as IGuildUser).Nickname;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> RoleIds
            => (RestWebhookUser as IGuildUser).RoleIds;

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel
            => (RestWebhookUser as IVoiceState).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => (RestWebhookUser as IVoiceState).VoiceSessionId;

        /// <inheritdoc />
        public ulong WebhookId
            => RestWebhookUser.WebhookId;

        /// <inheritdoc />
        public Task AddRoleAsync(IRole role, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).AddRoleAsync(role, options);

        /// <inheritdoc />
        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).AddRolesAsync(roles, options);

        /// <inheritdoc />
        public ChannelPermissions GetPermissions(IGuildChannel channel)
            => (RestWebhookUser as IGuildUser).GetPermissions(channel);

        /// <inheritdoc />
        public Task KickAsync(string reason = null, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).KickAsync(reason, options);

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).ModifyAsync(func, options);

        /// <inheritdoc />
        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).RemoveRoleAsync(role, options);

        /// <inheritdoc />
        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
            => (RestWebhookUser as IGuildUser).RemoveRolesAsync(roles, options);

        /// <summary>
        /// The existing <see cref="WebRest.RestWebhookUser"/> being abstracted.
        /// </summary>
        protected RestWebhookUser RestWebhookUser
            => RestUser as RestWebhookUser;

        public DateTimeOffset? PremiumSince => (RestUser as IGuildUser).PremiumSince;

        public bool IsStreaming => (RestUser as IGuildUser).IsStreaming;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestWebhookUser"/> objects.
    /// </summary>
    internal static class RestWebhookUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestWebhookUser"/> to an abstracted <see cref="IRestWebhookUser"/> value.
        /// </summary>
        /// <param name="restWebhookUser">The existing <see cref="RestWebhookUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restWebhookUser"/>.</exception>
        /// <returns>An <see cref="IRestWebhookUser"/> that abstracts <paramref name="restWebhookUser"/>.</returns>
        public static IRestWebhookUser Abstract(this RestWebhookUser restWebhookUser)
            => new RestWebhookUserAbstraction(restWebhookUser);
    }
}
