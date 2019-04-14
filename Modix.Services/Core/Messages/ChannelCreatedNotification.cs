using System;

using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.ChannelCreated"/> is raised.
    /// </summary>
    public class ChannelCreatedNotification : INotification
    {
        /// <summary>
        /// Constructs a new <see cref="ChannelCreatedNotification"/> from the given values.
        /// </summary>
        /// <param name="channel">The value to use for <see cref="Channel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="channel"/>.</exception>
        public ChannelCreatedNotification(ISocketChannel channel)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// The channel that was created.
        /// </summary>
        public ISocketChannel Channel { get; }
    }
}
