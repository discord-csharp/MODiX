using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;
using Npgsql;

namespace Modix.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<IReadOnlyDictionary<DateTime, int>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyDictionary<ulong, int>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyDictionary<GuildUserEntity, int>> GetPerUserMessageCounts(ulong guildId, TimeSpan timespan, int userCount = 10);

        Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId);

        Task CreateAsync(MessageEntity message);

        Task DeleteAsync(ulong messageId);
    }

    public class MessageRepository : RepositoryBase, IMessageRepository
    {
        public MessageRepository(ModixContext context)
            : base(context) { }

        public async Task CreateAsync(MessageEntity message)
        {
            await ModixContext.Messages.AddAsync(message);
            await ModixContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(ulong messageId)
        {
            if (await ModixContext.Messages.FindAsync(messageId) is MessageEntity message)
            {
                ModixContext.Messages.Remove(message);
                await ModixContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetGuildUserMessageCount(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime)
                .CountAsync();
        }

        public async Task<IReadOnlyDictionary<DateTime, int>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan)
        {
            return await GetGuildUserMessages(guildId, userId, timespan)
                .GroupBy(x => x.Timestamp.Date)
                .ToDictionaryAsync(x => x.Key, x => x.Count());
        }

        public async Task<IReadOnlyDictionary<ulong, int>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan)
        {
            return await GetGuildUserMessages(guildId, userId, timespan)
                .GroupBy(x => x.ChannelId)
                .ToDictionaryAsync(x => x.Key, x => x.Count());
        }

        public async Task<IReadOnlyDictionary<GuildUserEntity, int>> GetPerUserMessageCounts(ulong guildId, TimeSpan timespan, int userCount = 10)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            var result = await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.Timestamp >= earliestDateTime)
                .GroupBy(x => x.AuthorId)
                .OrderByDescending(x => x.Count())
                .Take(userCount)
                .ToListAsync();

            var userIds = result.Select(d => d.Key).ToArray();

            var userQuery = await ModixContext.GuildUsers
                .Where(x => x.GuildId == guildId)
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.User)
                .AsNoTracking()
                .ToDictionaryAsync(x => x.UserId, x => x);

            return result.ToDictionary(x => userQuery[x.Key], x => x.Count());
        }

        public async Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId)
        {
            var stats = await ModixContext.Query<GuildUserParticipationStatistics>()
                .AsNoTracking()
                .FromSql(
                    @"with msgs as (
                        select ""AuthorId"", ""Id"" as ""MessageId"", ""GuildId""
                        from ""Messages""
                        where ""GuildId"" = cast(:GuildId as numeric(20))
                        and ""Timestamp"" >= (current_date - interval '30 day')
                    ),
                    user_count as (
                        select ""AuthorId"", count(1) as ""MessageCount"", ""GuildId""
                        from msgs
                        group by ""AuthorId"", ""GuildId""
                    ),
                    user_avg as (
                        select ""AuthorId"", ""MessageCount"", (cast(""MessageCount"" as decimal) / cast(30 as decimal)) as ""AveragePerDay"", ""GuildId""
                        from user_count
                        group by ""AuthorId"", ""GuildId"", ""MessageCount""
                    ),
                    ntiles as (
                        select ""AuthorId"", ntile(100) over (order by ""AveragePerDay"") as ""Percentile"", ""GuildId""
                        from user_avg
                        group by ""AuthorId"", ""GuildId"", ""AveragePerDay""
                    ),
                    ranked_users as (
	                    select user_avg.""AuthorId"" as ""UserId"", ""AveragePerDay"", ""Percentile"", dense_rank() over (order by ""AveragePerDay"" desc) as ""Rank"", user_avg.""GuildId""
	                    from user_avg
	                    inner join ntiles on user_avg.""AuthorId"" = ntiles.""AuthorId"" and user_avg.""GuildId"" = ntiles.""GuildId""
                    )
                    select ""AveragePerDay"", ""Percentile"", ""Rank"", ""GuildId"", ""UserId""
                    from ranked_users
                    where ""UserId"" = cast(:UserId as numeric(20))",
                    new NpgsqlParameter(":GuildId", guildId.ToString()),
                    new NpgsqlParameter(":UserId", userId.ToString()))
                .OrderByDescending(x => x.AveragePerDay)
                .FirstOrDefaultAsync() ?? new GuildUserParticipationStatistics();

            stats.GuildId = guildId;
            stats.UserId = userId;

            return stats;
        }

        private IQueryable<MessageEntity> GetGuildUserMessages(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime);
        }
    }
}
