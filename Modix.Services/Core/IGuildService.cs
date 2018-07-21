using System.Threading.Tasks;

using Discord;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord guilds.
    /// </summary>
    public interface IGuildService
    {
        /// <summary>
        /// Retrieves the guild, if any, associated with the given Discord API ID value.
        /// </summary>
        /// <returns>The <see cref="IGuild"/>, if any, retrieved from Discord.NET.</returns>
        Task<IGuild> GetGuildAsync(ulong guildId);
    }
}
