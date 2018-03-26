using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace Modix.Services.GuildInfo
{
    public class GuildInfoService
    {
        private IMemoryCache _cache;

        private readonly MemoryCacheEntryOptions _cacheEntryOptions = 
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        public GuildInfoService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Create a unique key object for the cache
        /// </summary>
        private object GetKeyForGuild(IGuild guild) => new { guild, Target = "GuildInfo" };

        /// <summary>
        /// Gets a list of GuildInfoResult objects representing the role distriution for the given guild.
        /// </summary>
        /// <param name="guild">The guild to retrieve roles/counts from</param>
        /// <returns>A list of GuildInfoResult(s), each representing a role in the guild</returns>
        public async Task<List<GuildInfoResult>> GetGuildMemberDistribution(IGuild guild)
        {
            var key = GetKeyForGuild(guild);

            if (!_cache.TryGetValue(key, out List<GuildInfoResult> ret))
            {
                //Get all the server roles once, and memoize it
                var serverRoles = guild.Roles.ToDictionary(d => d.Id, d => d);

                var members = await guild.GetUsersAsync();

                //Group the users by their highest priority role (if they have one)
                var groupings = members.GroupBy(member => GetHighestRankingRole(serverRoles, member))
                    .Where(d=>d.Key != null);

                var roleCounts = groupings.OrderByDescending(d => d.Count());

                ret = roleCounts.Select(d => new GuildInfoResult
                {
                    Name = d.Key.Name,
                    Color = $"#{d.Key.Color.RawValue.ToString("X")}",
                    Count = d.Count()
                }).ToList();

                ret.Add(new GuildInfoResult { Name = "Other", Color = "#808080", Count = members.Count - ret.Sum(d => d.Count) });

                _cache.Set(key, ret, _cacheEntryOptions);
            }

            return ret;
        }

        /// <summary>
        /// Get the user's highest position role
        /// </summary>
        /// <param name="serverRoles">A dictionary of role IDs to roles in the server</param>
        /// <returns>The highest position role</returns>
        private IRole GetHighestRankingRole(IDictionary<ulong, IRole> serverRoles, IGuildUser user)
        {
            //Get the user's role from the cache
            var roles = user.RoleIds.Select(role => serverRoles[role]);

            //Try to get their highest role
            var highestPosition = roles
                .Where(d=>d.Name != "@everyone" && !d.IsManaged)
                .OrderByDescending(role => role.IsHoisted)
                .ThenByDescending(role => role.Position)
                .FirstOrDefault();

            return highestPosition;
        }
    }
}
