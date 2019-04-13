using System;

using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.GuildAvailable"/> is raised.
    /// </summary>
    public class GuildAvailableNotification : INotification
    {
        /// <summary>
        /// Constructs a new <see cref="GuildAvailableNotification"/> from the given values.
        /// </summary>
        /// <param name="guild">The value to use for <see cref="Guild"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guild"/>.</exception>
        public GuildAvailableNotification(ISocketGuild guild)
        {
            Guild = guild ?? throw new ArgumentNullException(nameof(guild));
        }

        /// <summary>
        /// The guild whose data is now available.
        /// </summary>
        public ISocketGuild Guild { get; }
    }
}
