using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestInviteMetadata" />
    public interface IRestInviteMetadata : IInviteMetadata, IInvite
    {
        /// <inheritdoc cref="RestInviteMetadata.Inviter" />
        new IRestUser Inviter { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestInviteMetadata"/>, through the <see cref="IRestInviteMetadata"/> interface.
    /// </summary>
    internal class RestInviteMetadataAbstraction : RestInviteAbstraction, IRestInviteMetadata
    {
        /// <summary>
        /// Constructs a new <see cref="RestInviteMetadataAbstraction"/> around an existing <see cref="Rest.RestInviteMetadata"/>.
        /// </summary>
        /// <param name="restInviteMetadata">The value to use for <see cref="Rest.RestInviteMetadata"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restInviteMetadata"/>.</exception>
        public RestInviteMetadataAbstraction(RestInviteMetadata restInviteMetadata)
            : base(restInviteMetadata) { }

        /// <inheritdoc />
        public DateTimeOffset? CreatedAt
            => RestInviteMetadata.CreatedAt;

        /// <inheritdoc />
        public IRestUser Inviter
            => RestInviteMetadata.Inviter
                .Abstract();

        /// <inheritdoc />
        IUser IInviteMetadata.Inviter
            => (RestInviteMetadata as IInviteMetadata).Inviter
                .Abstract();

        /// <inheritdoc />
        public bool IsRevoked
            => RestInviteMetadata.IsRevoked;

        /// <inheritdoc />
        public bool IsTemporary
            => RestInviteMetadata.IsTemporary;

        /// <inheritdoc />
        public int? MaxAge
            => RestInviteMetadata.MaxAge;

        /// <inheritdoc />
        public int? MaxUses
            => RestInviteMetadata.MaxUses;

        /// <inheritdoc />
        public int? Uses
            => RestInviteMetadata.Uses;

        /// <summary>
        /// The existing <see cref="Rest.RestInviteMetadata"/> being abstracted.
        /// </summary>
        protected RestInviteMetadata RestInviteMetadata
            => RestInvite as RestInviteMetadata;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestInviteMetadata"/> objects.
    /// </summary>
    internal static class RestInviteMetadataAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestInviteMetadata"/> to an abstracted <see cref="IRestInviteMetadata"/> value.
        /// </summary>
        /// <param name="restInviteMetadata">The existing <see cref="RestInviteMetadata"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restInviteMetadata"/>.</exception>
        /// <returns>An <see cref="IRestInviteMetadata"/> that abstracts <paramref name="restInviteMetadata"/>.</returns>
        public static IRestInviteMetadata Abstract(this RestInviteMetadata restInviteMetadata)
            => new RestInviteMetadataAbstraction(restInviteMetadata);
    }
}
