using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.UserBanned"/> is raised.
    /// </summary>
    public class UserBannedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="UserBannedNotification"/> from the given values.
        /// </summary>
        /// <param name="user">The value to use for <see cref="User"/>.</param>
        /// <param name="guild">The value to use for <see cref="Guild"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="user"/> and <paramref name="guild"/>.</exception>
        public UserBannedNotification(SocketUser user, SocketGuild guild)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Guild = guild ?? throw new ArgumentNullException(nameof(guild));
        }

        /// <summary>
        /// The user that was banned.
        /// </summary>
        public SocketUser User { get; }

        /// <summary>
        /// The guild from which the user was banned.
        /// </summary>
        public SocketGuild Guild { get; }
    }
}
