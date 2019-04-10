using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestInvite" />
    public interface IRestInvite : IInvite, IEntity<string>, IDeletable, IUpdateable { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestInvite"/>, through the <see cref="IRestInvite"/> interface.
    /// </summary>
    public class RestInviteAbstraction : IRestInvite
    {
        /// <summary>
        /// Constructs a new <see cref="RestInviteAbstraction"/> around an existing <see cref="Rest.RestInvite"/>.
        /// </summary>
        /// <param name="restInvite">The value to use for <see cref="Rest.RestInvite"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restInvite"/>.</exception>
        public RestInviteAbstraction(RestInvite restInvite)
        {
            RestInvite = restInvite ?? throw new ArgumentNullException(nameof(restInvite));
        }

        /// <inheritdoc />
        public IChannel Channel
            => (RestInvite as IInvite).Channel
                .Abstract();

        /// <inheritdoc />
        public ulong ChannelId
            => RestInvite.ChannelId;

        /// <inheritdoc />
        public ChannelType ChannelType
            => RestInvite.ChannelType;

        /// <inheritdoc />
        public string ChannelName
            => RestInvite.ChannelName;

        /// <inheritdoc />
        public string Code
            => RestInvite.Code;

        /// <inheritdoc />
        public IGuild Guild
            => (RestInvite as IInvite).Guild
                .Abstract();

        /// <inheritdoc />
        public ulong? GuildId
            => RestInvite.GuildId;

        /// <inheritdoc />
        public string GuildName
            => RestInvite.GuildName;

        /// <inheritdoc />
        public string Id
            => RestInvite.Id;

        /// <inheritdoc />
        public int? MemberCount
            => RestInvite.MemberCount;

        /// <inheritdoc />
        public int? PresenceCount
            => RestInvite.PresenceCount;

        /// <inheritdoc />
        public string Url
            => RestInvite.Url;

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestInvite.DeleteAsync(options);

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestInvite.UpdateAsync(options);

        /// <inheritdoc cref="RestInvite.ToString()"/>
        public override string ToString()
            => RestInvite.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestInvite"/> being abstracted.
        /// </summary>
        protected RestInvite RestInvite { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestInvite"/> objects.
    /// </summary>
    public static class RestInviteAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestInvite"/> to an abstracted <see cref="IRestInvite"/> value.
        /// </summary>
        /// <param name="restInvite">The existing <see cref="RestInvite"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restInvite"/>.</exception>
        /// <returns>An <see cref="IRestInvite"/> that abstracts <paramref name="restInvite"/>.</returns>
        public static IRestInvite Abstract(this RestInvite restInvite)
            => (restInvite is null) ? throw new ArgumentNullException(nameof(restInvite))
                : (restInvite is RestInviteMetadata restInviteMetadata) ? restInviteMetadata.Abstract() as IRestInvite
                : new RestInviteAbstraction(restInvite) as IRestInvite;
    }
}
