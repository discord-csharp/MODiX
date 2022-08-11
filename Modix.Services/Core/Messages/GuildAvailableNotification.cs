using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.GuildAvailable"/> is raised.
    /// </summary>
    public class GuildAvailableNotification
    {
        /// <summary>
        /// Constructs a new <see cref="GuildAvailableNotification"/> from the given values.
        /// </summary>
        /// <param name="guild">The value to use for <see cref="Guild"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guild"/>.</exception>
        public GuildAvailableNotification(SocketGuild guild)
        {
            Guild = guild ?? throw new ArgumentNullException(nameof(guild));
        }

        /// <summary>
        /// The guild whose data is now available.
        /// </summary>
        public SocketGuild Guild { get; }
    }
}
