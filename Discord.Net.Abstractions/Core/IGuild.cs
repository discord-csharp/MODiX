using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IGuild"/> objects.
    /// </summary>
    public static class GuildAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IGuild"/> to an abstracted <see cref="IGuild"/> value.
        /// </summary>
        /// <param name="guild">The existing <see cref="IGuild"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guild"/>.</exception>
        /// <returns>An <see cref="IGuild"/> that abstracts <paramref name="guild"/>.</returns>
        public static IGuild Abstract(this IGuild guild)
            => guild switch
            {
                null
                    => throw new ArgumentNullException(nameof(guild)),
                RestGuild restGuild
                    => RestGuildAbstractionExtensions.Abstract(restGuild) as IGuild,
                SocketGuild socketGuild
                    => SocketGuildAbstractionExtensions.Abstract(socketGuild) as IGuild,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IGuild)} type {guild.GetType().Name}")
            };
    }
}
