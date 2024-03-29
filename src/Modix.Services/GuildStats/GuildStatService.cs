using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.Caching.Memory;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.GuildStats
{
    public interface IGuildStatService
    {
        /// <summary>
        /// Gets a list of GuildInfoResult objects representing the role distribution for the given guild.
        /// </summary>
        /// <param name="guild">The guild to retrieve roles/counts from.</param>
        /// <returns>A list of GuildInfoResult(s), each representing a role in the guild.</returns>
        Task<List<GuildRoleCount>> GetGuildMemberDistributionAsync(IGuild guild);

        /// <summary>
        /// Returns a mapping of <see cref="GuildUserEntity"/> to a count of the messages they've sent.
        /// </summary>
        /// <param name="guild">The guild to count messages for.</param>
        /// <param name="userId">The Discord snowflake ID of the user who is querying for message counts.</param>
        Task<IReadOnlyCollection<PerUserMessageCount>> GetTopMessageCounts(IGuild guild, ulong userId);
    }

    public class GuildStatService :
        INotificationHandler<UserJoinedNotification>,
        INotificationHandler<UserLeftNotification>,
        IGuildStatService
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageRepository _messageRepository;

        private readonly MemoryCacheEntryOptions _roleCacheEntryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        private readonly MemoryCacheEntryOptions _msgCountCacheEntryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

        public GuildStatService(IMemoryCache cache, IMessageRepository messageRepository)
        {
            _cache = cache;
            _messageRepository = messageRepository;
        }

        /// <summary>
        /// Create a unique key object for the cache
        /// </summary>
        private object GetKeyForGuild(IGuild guild) => new { guild, Target = "GuildInfo" };

        /// <summary>
        /// Create a unique key object for the cache
        /// </summary>
        private object GetKeyForMsgCounts(IGuild guild, ulong userId) => new { guild, userId, Target = "MessageCounts" };

        /// <summary>
        /// Clear the cache entry for the given guild
        /// </summary>
        public void ClearCacheEntry(IGuild guild)
        {
            _cache.Remove(GetKeyForGuild(guild));
        }

        public Task HandleNotificationAsync(UserJoinedNotification notification, CancellationToken cancellationToken)
        {
            ClearCacheEntry(notification.GuildUser.Guild);
            return Task.CompletedTask;
        }

        public Task HandleNotificationAsync(UserLeftNotification notification, CancellationToken cancellationToken)
        {
            ClearCacheEntry(notification.Guild);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PerUserMessageCount>> GetTopMessageCounts(IGuild guild, ulong userId)
        {
            var key = GetKeyForMsgCounts(guild, userId);

            if (!_cache.TryGetValue(key, out IReadOnlyCollection<PerUserMessageCount> ret))
            {
                ret = await _messageRepository.GetPerUserMessageCounts(guild.Id, userId, TimeSpan.FromDays(30));
                _cache.Set(key, ret, _msgCountCacheEntryOptions);
            }

            return ret;
        }

        /// <inheritdoc />
        public async Task<List<GuildRoleCount>> GetGuildMemberDistributionAsync(IGuild guild)
        {
            var key = GetKeyForGuild(guild);

            if (!_cache.TryGetValue(key, out List<GuildRoleCount> ret))
            {
                //Get all the server roles once, and memoize it
                var serverRoles = guild.Roles.ToDictionary(d => d.Id, d => d);

                var members = await guild.GetUsersAsync();

                //Group the users by their highest priority role (if they have one)
                var groupings = members.GroupBy(member => GetHighestRankingRole(serverRoles, member))
                    .Where(d => d.Key != null);

                var roleCounts = groupings.OrderByDescending(d => d.Count());

                ret = roleCounts.Select(d => new GuildRoleCount
                {
                    Name = d.Key.Name,
                    Color = GetRoleColorHex(d.Key),
                    Count = d.Count()
                }).ToList();

                _cache.Set(key, ret, _roleCacheEntryOptions);
            }

            return ret;
        }

        public string GetRoleColorHex(IRole role)
        {
            if (role.Color.RawValue > 0)
            {
                return role.Color.ToString();
            }

            return Color.Default.ToString();
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
                .Where(d =>
                    d.Name != "@everyone"
                    && !d.IsManaged
                    && d.Color != Color.Default)
                .OrderByDescending(role => role.IsHoisted)
                .ThenByDescending(role => role.Position)
                .FirstOrDefault();

            return highestPosition;
        }
    }
}
