using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.ChannelUpdated"/> is raised.
    /// </summary>
    public class ChannelUpdatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelUpdatedNotification"/> from the given values.
        /// </summary>
        /// <param name="oldChannel">The value to use for <see cref="OldChannel"/>.</param>
        /// <param name="newChannel">The value to use for <see cref="NewChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="oldChannel"/> and <paramref name="newChannel"/>.</exception>
        public ChannelUpdatedNotification(SocketChannel oldChannel, SocketChannel newChannel)
        {
            OldChannel = oldChannel ?? throw new ArgumentNullException(nameof(oldChannel));
            NewChannel = newChannel ?? throw new ArgumentNullException(nameof(newChannel));
        }

        /// <summary>
        /// The state of the channel that was updated, prior to the update.
        /// </summary>
        public SocketChannel OldChannel { get; }

        /// <summary>
        /// The state of the channel that was updated, after the update.
        /// </summary>
        public SocketChannel NewChannel { get; }
    }
}
