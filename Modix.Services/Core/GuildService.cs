using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
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
        /// <param name="allowCache">Indicates whether to look for the guild in the cache before querying the Discord API. May result in outdated information.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> that completes when the operation completes,
        /// containing all information that was found for the guild.
        /// </returns>
        ValueTask<GuildResult> GetGuildInformationAsync(ulong guildId, bool allowCache = false);
    }

    /// <inheritdoc />
    internal sealed class GuildService : IGuildService
    {
        /// <summary>
        /// Constructs a new <see cref="GuildService"/> with the given injected dependencies.
        /// </summary>
        public GuildService(
            DiscordRestClient discordRestClient,
            DiscordSocketClient socketClient,
            IMemoryCache cache)
        {
            DiscordRestClient = discordRestClient;
            DiscordSocketClient = socketClient;
            _cache = cache;
        }

        /// <inheritdoc />
        public async ValueTask<GuildResult> GetGuildInformationAsync(ulong guildId, bool allowCache = false)
        {
            var key = GetKey(guildId);

            if (!allowCache || !_cache.TryGetValue<IGuild>(key, out var guild))
            {
                guild = DiscordSocketClient.GetGuild(guildId);
            }

            if (guild is { })
            {
                _cache.Set(key, guild, TimeSpan.FromDays(7));

                return new GuildResult(guild);
            }

            try
            {
                guild = await DiscordRestClient.GetGuildAsync(guildId);
            }
            catch (HttpException ex) when (ex.DiscordCode == 50001)
            {
                return new GuildResult("Sorry, I do not have access to any guilds with that ID.");
            }

            if (guild is { })
            {
                _cache.Set(key, guild, TimeSpan.FromDays(7));
            }

            return new GuildResult(guild);
        }

        /// <summary>
        /// A <see cref="DiscordRestClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal DiscordRestClient DiscordRestClient { get; }

        /// <summary>
        /// A <see cref="DiscordSocketClient"/> to be used to interact with the Discord API.
        /// </summary>
        internal DiscordSocketClient DiscordSocketClient { get; }

        private object GetKey(ulong guildId)
            => new { Target = "Guild", guildId };

        private readonly IMemoryCache _cache;
    }
}
