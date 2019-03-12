using System.Threading.Tasks;

using Discord.Net;
using Discord.Rest;

using Modix.Data.Models.Core;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord guilds.
    /// </summary>
    public interface IGuildService
    {
        /// <summary>
        /// Retrieves all available information on a guild matching the supplied criteria.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild being searched.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the operation completes,
        /// containing all information that was found for the guild.
        /// </returns>
        Task<GuildResult> GetGuildInformationAsync(ulong guildId);
    }

    /// <inheritdoc />
    internal sealed class GuildService : IGuildService
    {
        /// <summary>
        /// Constructs a new <see cref="GuildService"/> with the given injected dependencies.
        /// </summary>
        public GuildService(DiscordRestClient discordRestClient)
        {
            DiscordRestClient = discordRestClient;
        }

        /// <inheritdoc />
        public async Task<GuildResult> GetGuildInformationAsync(ulong guildId)
        {
            RestGuild restGuild;

            try
            {
                restGuild = await DiscordRestClient.GetGuildAsync(guildId);
            }
            catch (HttpException ex) when (ex.DiscordCode == 50001)
            {
                return new GuildResult("Sorry, I do not have access to any guilds with that ID.");
            }

            return new GuildResult(restGuild);
        }

        /// <summary>
        /// A <see cref="DiscordRestClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal DiscordRestClient DiscordRestClient { get; }
    }
}
