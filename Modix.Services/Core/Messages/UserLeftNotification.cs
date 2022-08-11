using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.UserLeft"/> is raised.
    /// </summary>
    public class UserLeftNotification
    {
        /// <summary>
        /// Constructs a new <see cref="UserLeftNotification"/> from the given values.
        /// </summary>
        /// <param name="guildUser">The value to use for <see cref="GuildUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildUser"/>.</exception>
        public UserLeftNotification(SocketGuildUser guildUser)
        {
            GuildUser = guildUser ?? throw new ArgumentNullException(nameof(guildUser));
        }

        /// <summary>
        /// The user that left, and the guild that was left.
        /// </summary>
        public SocketGuildUser GuildUser { get; }
    }
}
