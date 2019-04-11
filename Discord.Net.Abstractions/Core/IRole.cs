using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IRole"/> objects.
    /// </summary>
    internal static class RoleAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IRole"/> to an abstracted <see cref="IRole"/> value.
        /// </summary>
        /// <param name="role">The existing <see cref="IRole"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="role"/>.</exception>
        /// <returns>An <see cref="IRole"/> that abstracts <paramref name="role"/>.</returns>
        public static IRole Abstract(this IRole role)
            => role switch
            {
                null
                    => throw new ArgumentNullException(nameof(role)),
                RestRole restRole
                    => RestRoleAbstractionExtensions.Abstract(restRole) as IRole,
                SocketRole socketRole
                    => SocketRoleAbstractionExtensions.Abstract(socketRole) as IRole,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IRole)} type {role.GetType().Name}")
            };
    }
}
