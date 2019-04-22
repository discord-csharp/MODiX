﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;
using Npgsql;
using NpgsqlTypes;

namespace Modix.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<IReadOnlyDictionary<DateTime, int>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyDictionary<ulong, int>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan);

        Task<IReadOnlyCollection<PerUserMessageCount>> GetPerUserMessageCounts(ulong guildId, ulong userId, TimeSpan timespan, int userCount = 10);

        Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId);

        Task CreateAsync(MessageEntity message);

        Task DeleteAsync(ulong messageId);

        Task<MessageEntity> GetMessage(ulong messageId);

        Task UpdateStarboardColumn(MessageEntity message);

        Task<int> GetTotalMessageCountAsync(ulong guildId, TimeSpan timespan);

        Task<IReadOnlyDictionary<ulong, int>> GetTotalMessageCountByChannelAsync(ulong guildId, TimeSpan timespan);
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
            if (await GetMessage(messageId) is MessageEntity message)
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

        public async Task<IReadOnlyCollection<PerUserMessageCount>> GetPerUserMessageCounts(ulong guildId, ulong userId, TimeSpan timespan, int userCount = 10)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;
            var query = GetQuery();

            var counts = await ModixContext.Query<PerUserMessageCount>()
                .AsNoTracking()
                .FromSql(query,
                    new NpgsqlParameter(":GuildId", NpgsqlDbType.Bigint) { Value = unchecked((long)guildId) },
                    new NpgsqlParameter(":UserId", NpgsqlDbType.Bigint) { Value = unchecked((long)userId) },
                    new NpgsqlParameter(":StartTimestamp", NpgsqlDbType.TimestampTz) { Value = earliestDateTime })
                .ToArrayAsync();

            return counts;

            string GetQuery()
                => $@"
                    with guildCounts as (
                        select ""AuthorId"" as ""UserId"",
                            row_number() over (order by count(*) desc) as ""Rank"",
                            count(*) as ""MessageCount"",
                            ""AuthorId"" = :UserId as ""IsCurrentUser""
                        from ""Messages""
                        where ""GuildId"" = :GuildId
                            and ""Timestamp"" >= :StartTimestamp
                        group by ""AuthorId"", ""GuildId""
                    ),
                    currentUserCount as (
                        select ""UserId"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                        from guildCounts
                        where ""UserId"" = :UserId
                    ),
                    guildCountsLimited as (
                        select ""UserId"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                        from guildCounts
                        limit {userCount}
                    ),
                    unioned as (
                        select ""UserId"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                        from guildCountsLimited
                        union select ""UserId"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                        from currentUserCount
                    ),
                    joined as (
                        select ""Username"", ""Discriminator"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                        from unioned
                        inner join ""Users""
                            on ""Id"" = ""UserId""
                    )
                    select ""Username"", ""Discriminator"", ""Rank"", ""MessageCount"", ""IsCurrentUser""
                    from joined
                    order by ""Rank"" asc";
        }

        public async Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId)
        {
            var stats = await ModixContext.Query<GuildUserParticipationStatistics>()
                .AsNoTracking()
                .FromSql(
                    @"with msgs as (
                        select msg.""AuthorId"", msg.""Id"" as ""MessageId"", msg.""GuildId""
                        from ""Messages"" as msg
                        left outer join ""DesignatedChannelMappings"" as dcm on msg.""ChannelId"" = dcm.""ChannelId""
                        where msg.""GuildId"" = cast(:GuildId as numeric(20))
                        and dcm.""Type"" = 'CountsTowardsParticipation'
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

        public async Task<MessageEntity> GetMessage(ulong messageId)
        {
            return await ModixContext.Messages.FindAsync(messageId);
        }

        public async Task UpdateStarboardColumn(MessageEntity message)
        {
            ModixContext.Messages.Update(message);
            await ModixContext.SaveChangesAsync();
        }

        public async Task<int> GetTotalMessageCountAsync(ulong guildId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId
                    && x.Timestamp >= earliestDateTime)
                .CountAsync();
        }

        public async Task<IReadOnlyDictionary<ulong, int>> GetTotalMessageCountByChannelAsync(ulong guildId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return await ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId
                    && x.Timestamp >= earliestDateTime)
                .GroupBy(x => x.ChannelId)
                .ToDictionaryAsync(x => x.Key, x => x.Count());
        }

        private IQueryable<MessageEntity> GetGuildUserMessages(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return ModixContext.Messages.AsNoTracking()
                .Where(x => x.GuildId == guildId && x.AuthorId == userId && x.Timestamp >= earliestDateTime);
        }
    }
}
