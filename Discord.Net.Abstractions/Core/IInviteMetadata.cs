using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IInviteMetadata"/> objects.
    /// </summary>
    public static class InviteMetadataAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IInviteMetadata"/> to an abstracted <see cref="IInviteMetadata"/> value.
        /// </summary>
        /// <param name="inviteMetadata">The existing <see cref="IInviteMetadata"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="inviteMetadata"/>.</exception>
        /// <returns>An <see cref="IInviteMetadata"/> that abstracts <paramref name="inviteMetadata"/>.</returns>
        public static IInviteMetadata Abstract(this IInviteMetadata inviteMetadata)
            => inviteMetadata switch
            {
                null
                    => throw new ArgumentNullException(nameof(inviteMetadata)),
                RestInviteMetadata restInviteMetadata
                    => RestInviteMetadataAbstractionExtensions.Abstract(restInviteMetadata) as IInviteMetadata,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IInviteMetadata)} type {inviteMetadata.GetType().Name}")
            };
    }
}
