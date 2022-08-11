#nullable enable

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.GuildMemberUpdated"/> is raised.
    /// </summary>
    public class GuildMemberUpdatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="GuildMemberUpdatedNotification"/> from the given values.
        /// </summary>
        /// <param name="oldMember">The value to use for <see cref="OldMember"/>.</param>
        /// <param name="newMember">The value to use for <see cref="NewMember"/>.</param>
        public GuildMemberUpdatedNotification(
            Cacheable<SocketGuildUser, ulong> oldMember,
            SocketGuildUser newMember)
        {
            OldMember = oldMember;
            NewMember = newMember;
        }

        /// <summary>
        /// A model of the Guild Member that was updated, from before the update.
        /// </summary>
        public Cacheable<SocketGuildUser, ulong> OldMember { get; }

        /// <summary>
        /// A model of the Guild Member that was updated, from after the update.
        /// </summary>
        public SocketGuildUser NewMember { get; }
    }
}
