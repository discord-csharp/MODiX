using System;

using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Describes an application-wide notification that occurs when <see cref="IBaseSocketClient.VoiceChannelStatusUpdated"/> is raised.
    /// </summary>
    public class VoiceChannelStatusUpdatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="VoiceChannelStatusUpdatedNotification"/> from the given values.
        /// </summary>
        /// <param name="channel">The value to use for <see cref="Channel"/>.</param>
        /// <param name="oldStatus">The value to use for <see cref="OldStatus"/>.</param>
        /// <param name="newStatus">The value to use for <see cref="NewStatus"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="oldStatus"/>.</exception>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="newStatus"/>.</exception>
        public VoiceChannelStatusUpdatedNotification(Cacheable<SocketVoiceChannel, ulong> channel, string oldStatus, string newStatus)
        {
            ArgumentNullException.ThrowIfNull(oldStatus);
            ArgumentNullException.ThrowIfNull(newStatus);

            Channel = channel;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
        /// <summary>
        /// The voice channel whose status was updated.
        /// </summary>
        public Cacheable<SocketVoiceChannel, ulong> Channel { get; }
        /// <summary>
        /// The old voice channel status.
        /// </summary>
        public string OldStatus { get; }
        /// <summary>
        /// The new voice channel status.
        /// </summary>
        public string NewStatus { get; }
    }
}
