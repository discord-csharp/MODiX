using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;
using Npgsql;
using NpgsqlTypes;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing message entities within an underlying data storage provider.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Begins a new transaction to maintain message entities within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginMaintainTransactionAsync();

        /// <summary>
        /// Creates a new message log within the repository.
        /// </summary>
        /// <param name="data">The data for the message log to be created.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task CreateAsync(MessageCreationData data);

        /// <summary>
        /// Deletes emoji logs within the repository.
        /// </summary>
        /// <param name="messageId">The unique Discord snowflake ID of the message to delete.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task DeleteAsync(ulong messageId);

        /// <summary>
        /// Searches the message logs for message records matching the supplied ID.
        /// </summary>
        /// <param name="messageId">The unique Discord snowflake ID of the message to look for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a <see cref="MessageBrief"/> containing information for the message or a default value if no match is found.
        /// </returns>
        Task<MessageBrief?> GetMessage(ulong messageId);

        /// <summary>
        /// Searches the message logs for message records matching the supplied guild ID and user ID within a given timeframe.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild to search in.</param>
        /// <param name="userId">The unique Discord snowflake ID of the user to get a message count for.</param>
        /// <param name="timespan">The timeframe the query should be based on.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a dictionary which contains a separate message count for each day within the given timeframe.
        /// </returns>
        Task<IReadOnlyList<MessageCountByDate>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan);

        /// <summary>
        /// Searches the message logs for message records matching the supplied guild ID and user ID within a given timeframe.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild to search in.</param>
        /// <param name="userId">The unique Discord snowflake ID of the user to get a message count for.</param>
        /// <param name="timespan">The timeframe the query should be based on.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a dictionary which contains the number of messages the given user has sent in each channel within the given timeframe.
        /// </returns>
        Task<IReadOnlyList<MessageCountPerChannel>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan);

        /// <summary>
        /// Calculates a given number of users contributions (through messages) in a given guild in a specific timeframe.
        /// </summary>
        /// <param name="guildId">The guild to count messages for.</param>
        /// <param name="userId">The Discord snowflake ID of the user who is querying for message counts.</param>
        /// <param name="timespan">The timeframe the query should be based on.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of the given size <paramref name="userCount"/> within the given timeframe including the user who requested the information.
        /// </returns>
        Task<IReadOnlyCollection<PerUserMessageCount>> GetPerUserMessageCounts(ulong guildId, ulong userId, TimeSpan timespan, int userCount = 10);

        /// <summary>
        /// Gets participation information about a given user in a given guild.
        /// </summary>
        /// <param name="guildId">The guild to count messages for.</param>
        /// <param name="userId">The Discord snowflake ID of the user who is querying for message counts.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing <see cref="GuildUserParticipationStatistics"/> which contains information about the given user's participation in the given guild.
        /// </returns>
        Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId);

        /// <summary>
        /// Updates the starboard-entry column for a given message ID.
        /// </summary>
        /// <param name="messageId">The unique Discord snowflake ID of the message to update the starboard-entry column for.</param>
        /// <param name="starboardEntryId">The ID of the starboard-entry message. Optionally null if the record no longer should exist.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateStarboardColumn(ulong messageId, ulong? starboardEntryId);

        /// <summary>
        /// Gets the total amount of messages logged for a given guild in a certain timeframe.
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="timespan"></param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the number of messages sent in the given guild during the given time period.</returns>
        Task<int> GetTotalMessageCountAsync(ulong guildId, TimeSpan timespan);

        /// <summary>
        /// Gets the total amount of messages sent in each channel of a given guild during a specific timeframe.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild to search in.</param>
        /// <param name="timespan">The timeframe the query should be based on.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a dictionary which contains a separate message count for each channel within the given timeframe.
        /// </returns>
        Task<IReadOnlyDictionary<ulong, int>> GetTotalMessageCountByChannelAsync(ulong guildId, TimeSpan timespan);
    }

    public sealed class MessageRepository : RepositoryBase, IMessageRepository
    {
        public MessageRepository(ModixContext context)
            : base(context) { }

        private static readonly RepositoryTransactionFactory _maintainTransactionFactory
            = new RepositoryTransactionFactory();

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginMaintainTransactionAsync()
            => _maintainTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(MessageCreationData data)
        {
            var entity = data.ToEntity();
            ModixContext.Set<MessageEntity>().Add(entity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(ulong messageId)
        {
            var entity = await ModixContext.Set<MessageEntity>()
                .Where(x => x.Id == messageId)
                .FirstOrDefaultAsync();

            if (entity is not null)
            {
                ModixContext.Set<MessageEntity>().Remove(entity);
                await ModixContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MessageCountByDate>> GetGuildUserMessageCountByDate(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;
            var results = await ModixContext.Database
                .SqlQuery<MessageCountByDate>(
                    $@"select date(""Timestamp""::timestamp AT TIME ZONE 'UTC') as ""Date"", count(""Id"") as ""MessageCount""
                      from ""Messages""
                      where ""GuildId"" = {unchecked((long)guildId)}
                      and ""AuthorId"" = {unchecked((long)userId)}
                      and ""Timestamp"" > {earliestDateTime}
                      group by date(""Timestamp""::timestamp AT TIME ZONE 'UTC')")
                .ToArrayAsync();

            return results;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MessageCountPerChannel>> GetGuildUserMessageCountByChannel(ulong guildId, ulong userId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            var counts = await ModixContext.Database
                .SqlQuery<MessageCountPerChannel>(
                    $@"with user_messages as
                    (
                        select coalesce(c.""ParentChannelId"", c.""ChannelId"") as ""ChannelId"", count(""Id"") as ""MessageCount""
                        from ""Messages"" as m
                        inner join ""GuildChannels"" as c on m.""ChannelId"" = c.""ChannelId""
                        where m.""GuildId"" = {unchecked((long)guildId)}
                        and m.""AuthorId"" = {unchecked((long)userId)}
                        and m.""Timestamp"" > {earliestDateTime}
                        group by coalesce(c.""ParentChannelId"", c.""ChannelId"")
                    )
                    select gc.""ChannelId"", gc.""Name"" as ""ChannelName"", um.""MessageCount""
                    from ""GuildChannels"" as gc
                    inner join user_messages as um on gc.""ChannelId"" = um.""ChannelId""
                    order by um.""MessageCount"" desc")
                .ToArrayAsync();

            return counts;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PerUserMessageCount>> GetPerUserMessageCounts(ulong guildId, ulong userId, TimeSpan timespan, int userCount = 10)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;
            var query = GetQuery();

            var counts = await ModixContext.Database
                .SqlQueryRaw<PerUserMessageCount>(query,
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
                        inner join ""GuildChannels"" on ""GuildChannels"".""ChannelId"" = ""Messages"".""ChannelId""
                        inner join ""DesignatedChannelMappings"" on coalesce(""GuildChannels"".""ParentChannelId"", ""GuildChannels"".""ChannelId"") = ""DesignatedChannelMappings"".""ChannelId""
                        where ""Messages"".""GuildId"" = :GuildId
                            and ""DesignatedChannelMappings"".""Type"" = 'CountsTowardsParticipation'
                            and ""Timestamp"" >= :StartTimestamp
                        group by ""AuthorId"", ""Messages"".""GuildId""
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

        /// <inheritdoc />
        public async Task<GuildUserParticipationStatistics> GetGuildUserParticipationStatistics(ulong guildId, ulong userId)
        {
            var stats = await ModixContext.Database
                .SqlQuery<GuildUserParticipationStatistics>(
                    $@"with msgs as (
                        select msg.""AuthorId"", msg.""Id"" as ""MessageId"", msg.""GuildId""
                        from ""Messages"" as msg
                        inner join ""GuildChannels"" as c on c.""ChannelId"" = msg.""ChannelId""
                        inner join ""DesignatedChannelMappings"" as dcm on coalesce(c.""ParentChannelId"", c.""ChannelId"") = dcm.""ChannelId""
                        where msg.""GuildId"" = {unchecked((long)guildId)}
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
                    where ""UserId"" = {unchecked((long)userId)}")
                .OrderByDescending(x => x.AveragePerDay)
                .FirstOrDefaultAsync();

            return stats ?? new()
            {
                GuildId = guildId,
                UserId = userId
            };
        }

        /// <inheritdoc />
        public async Task<MessageBrief?> GetMessage(ulong messageId)
        {
            return await ModixContext.Set<MessageEntity>()
                .AsNoTracking()
                .Where(x => x.Id == messageId)
                .Select(MessageBrief.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task UpdateStarboardColumn(ulong messageId, ulong? starboardEntryId)
        {
            var entity = await ModixContext.Set<MessageEntity>()
                .Where(x => x.Id == messageId)
                .FirstAsync();

            entity.StarboardEntryId = starboardEntryId;

            ModixContext.Set<MessageEntity>().Update(entity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetTotalMessageCountAsync(ulong guildId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            return await ModixContext.Set<MessageEntity>().AsNoTracking()
                .Where(x => x.GuildId == guildId
                    && x.Timestamp >= earliestDateTime)
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<ulong, int>> GetTotalMessageCountByChannelAsync(ulong guildId, TimeSpan timespan)
        {
            var earliestDateTime = DateTimeOffset.UtcNow - timespan;

            var messages = await ModixContext.Set<MessageEntity>()
                .AsNoTracking()
                .Where(x => x.GuildId == guildId && x.Timestamp >= earliestDateTime)
                .Select(x => x.Channel.ParentChannelId ?? x.ChannelId)
                .ToListAsync();

            return messages
                .GroupBy(channelId => channelId)
                .ToDictionary(x => x.Key, x => x.Count());
        }
    }
}
