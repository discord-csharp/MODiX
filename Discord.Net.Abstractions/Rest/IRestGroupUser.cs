using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGroupUser" />
    public interface IRestGroupUser : IRestUser, IGroupUser { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGroupUser"/>, through the <see cref="IRestGroupUser"/> interface.
    /// </summary>
    public class RestGroupUserAbstraction : RestUserAbstraction, IRestGroupUser
    {
        /// <summary>
        /// Constructs a new <see cref="RestGroupUserAbstraction"/> around an existing <see cref="Rest.RestGroupUser"/>.
        /// </summary>
        /// <param name="restGroupUser">The value to use for <see cref="Rest.RestGroupUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGroupUser"/>.</exception>
        public RestGroupUserAbstraction(RestGroupUser restGroupUser)
            : base(restGroupUser) { }

        /// <inheritdoc />
        public bool IsDeafened
            => (RestGroupUser as IVoiceState).IsDeafened;

        /// <inheritdoc />
        public bool IsMuted
            => (RestGroupUser as IVoiceState).IsMuted;

        /// <inheritdoc />
        public bool IsSelfDeafened
            => (RestGroupUser as IVoiceState).IsSelfDeafened;

        /// <inheritdoc />
        public bool IsSelfMuted
            => (RestGroupUser as IVoiceState).IsMuted;

        /// <inheritdoc />
        public bool IsSuppressed
            => (RestGroupUser as IVoiceState).IsSuppressed;

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel
            => (RestGroupUser as IVoiceState).VoiceChannel
                .Abstract();

        /// <inheritdoc />
        public string VoiceSessionId
            => (RestGroupUser as IVoiceState).VoiceSessionId;

        /// <summary>
        /// The existing <see cref="Rest.RestGroupUser"/> being abstracted.
        /// </summary>
        protected RestGroupUser RestGroupUser
            => RestUser as RestGroupUser;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGroupUser"/> objects.
    /// </summary>
    public static class RestGroupUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGroupUser"/> to an abstracted <see cref="IRestGroupUser"/> value.
        /// </summary>
        /// <param name="restGroupUser">The existing <see cref="RestGroupUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGroupUser"/>.</exception>
        /// <returns>An <see cref="IRestGroupUser"/> that abstracts <paramref name="restGroupUser"/>.</returns>
        public static IRestGroupUser Abstract(this RestGroupUser restGroupUser)
            => new RestGroupUserAbstraction(restGroupUser);
    }
}
