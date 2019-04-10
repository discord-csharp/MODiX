using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IUser"/> objects.
    /// </summary>
    internal static class UserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IUser"/> to an abstracted <see cref="IUser"/> value.
        /// </summary>
        /// <param name="user">The existing <see cref="IUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="user"/>.</exception>
        /// <returns>An <see cref="IUser"/> that abstracts <paramref name="user"/>.</returns>
        public static IUser Abstract(this IUser user)
            => user switch
            {
                null
                    => throw new ArgumentNullException(nameof(user)),
                RestUser restUser
                    => RestUserAbstractionExtensions.Abstract(restUser) as IUser,
                SocketUser socketUser
                    => SocketUserAbstractionExtensions.Abstract(socketUser) as IUser,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IUser)} type {user.GetType().Name}")
            };
    }
}
