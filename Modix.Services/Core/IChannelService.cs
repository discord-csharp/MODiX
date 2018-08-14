using System.Threading.Tasks;

using Discord;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord channels, within the application.
    /// </summary>
    public interface IChannelService
    {
        /// <summary>
        /// Updates information about the given channel within the channel tracking..
        /// </summary>
        /// <param name="channel">The channel whose info is to be tracked.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackChannelAsync(IGuildChannel channel);
    }
}
