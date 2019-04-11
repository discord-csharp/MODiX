using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IGuildUser"/> objects.
    /// </summary>
    internal static class GuildUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IGuildUser"/> to an abstracted <see cref="IGuildUser"/> value.
        /// </summary>
        /// <param name="guildUser">The existing <see cref="IGuildUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildUser"/>.</exception>
        /// <returns>An <see cref="IGuildUser"/> that abstracts <paramref name="guildUser"/>.</returns>
        public static IGuildUser Abstract(this IGuildUser guildUser)
            => guildUser switch
            {
                null
                    => throw new ArgumentNullException(nameof(guildUser)),
                RestGuildUser restGuildUser
                    => RestGuildUserAbstractionExtensions.Abstract(restGuildUser) as IGuildUser,
                SocketGuildUser socketGuildUser
                    => SocketGuildUserAbstractionExtensions.Abstract(socketGuildUser) as IGuildUser,
                SocketWebhookUser socketWebhookUser
                    => SocketWebhookUserAbstractionExtensions.Abstract(socketWebhookUser) as IGuildUser,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IGuildUser)} type {guildUser.GetType().Name}")
            };
    }
}
