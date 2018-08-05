using System.Threading.Tasks;

using Discord;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves the user, if any, associated with the given Discord ID value.
        /// </summary>
        /// <param name="userId">The <see cref="IEntity{T}.Id" /> of the user to be retrieved.</param>
        /// <returns>
        /// The <see cref="IUser"/>, if any, retrieved from Discord.NET.
        /// This user may also be an <see cref="IGuildUser"/>, if the current request is associated with a particular guild.
        /// </returns>
        Task<IUser> GetUserAsync(ulong userId);

        /// <summary>
        /// Retrieves the user, if any, associated with the given Discord ID value that also belongs to a specified guild.
        /// </summary>
        /// <param name="guildId">The <see cref="IEntity{T}.Id" /> of the guild whose user is to be retrieved.</param>
        /// <param name="userId">The <see cref="IEntity{T}.Id" /> of the user to be retrieved.</param>
        /// <returns>The <see cref="IGuildUser"/>, if any, retrieved from Discord.NET.</returns>
        Task<IGuildUser> GetGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Updates information about the given user within the user tracking system of a guild.
        /// </summary>
        /// <param name="user">The user whose info is to be tracked.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task TrackUserAsync(IGuildUser user);
    }
}
