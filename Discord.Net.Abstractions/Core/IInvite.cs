using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IInvite"/> objects.
    /// </summary>
    internal static class InviteAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IInvite"/> to an abstracted <see cref="IInvite"/> value.
        /// </summary>
        /// <param name="invite">The existing <see cref="IInvite"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="invite"/>.</exception>
        /// <returns>An <see cref="IInvite"/> that abstracts <paramref name="invite"/>.</returns>
        public static IInvite Abstract(this IInvite invite)
            => invite switch
            {
                null
                    => throw new ArgumentNullException(nameof(invite)),
                RestInviteMetadata restInviteMetadata
                    => RestInviteMetadataAbstractionExtensions.Abstract(restInviteMetadata) as IInvite,
                RestInvite restInvite
                    => RestInviteAbstractionExtensions.Abstract(restInvite) as IInvite,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IInvite)} type {invite.GetType().Name}")
            };
    }
}
