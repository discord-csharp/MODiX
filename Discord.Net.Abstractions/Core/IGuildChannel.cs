using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IGuildChannel"/> objects.
    /// </summary>
    public static class GuildChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IGuildChannel"/> to an abstracted <see cref="IGuildChannel"/> value.
        /// </summary>
        /// <param name="guildChannel">The existing <see cref="IGuildChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildChannel"/>.</exception>
        /// <returns>An <see cref="IGuildChannel"/> that abstracts <paramref name="guildChannel"/>.</returns>
        public static IGuildChannel Abstract(this IGuildChannel guildChannel)
            => guildChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(guildChannel)),
                RestGuildChannel restGuildChannel
                    => RestGuildChannelAbstractionExtensions.Abstract(restGuildChannel) as IGuildChannel,
                SocketGuildChannel socketGuildChannel
                    => SocketGuildChannelAbstractionExtensions.Abstract(socketGuildChannel) as IGuildChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IGuildChannel)} type {guildChannel.GetType().Name}")
            };
    }
}
