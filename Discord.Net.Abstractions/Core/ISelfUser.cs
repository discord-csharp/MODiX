using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="ISelfUser"/> objects.
    /// </summary>
    public static class SelfUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ISelfUser"/> to an abstracted <see cref="ISelfUser"/> value.
        /// </summary>
        /// <param name="selfUser">The existing <see cref="ISelfUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="selfUser"/>.</exception>
        /// <returns>An <see cref="ISelfUser"/> that abstracts <paramref name="selfUser"/>.</returns>
        public static ISelfUser Abstract(this ISelfUser selfUser)
            => selfUser switch
            {
                null
                    => throw new ArgumentNullException(nameof(selfUser)),
                RestSelfUser restSelfUser
                    => RestSelfUserAbstractionExtensions.Abstract(restSelfUser) as ISelfUser,
                SocketSelfUser socketSelfUser
                    => SocketSelfUserAbstractionExtensions.Abstract(socketSelfUser) as ISelfUser,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(ISelfUser)} type {selfUser.GetType().Name}")
            };
    }
}
