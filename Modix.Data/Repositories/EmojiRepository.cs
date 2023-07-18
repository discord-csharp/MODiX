using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Npgsql;
using NpgsqlTypes;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing emoji entities within an underlying data storage provider.
    /// </summary>
    public interface IEmojiRepository
    {
        /// <summary>
        /// Begins a new transaction to maintain emoji within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginMaintainTransactionAsync();

        /// <summary>
        /// Creates a new emoji log within the repository.
        /// </summary>
        /// <param name="data">The data for the emoji log to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated identifier value assigned to the new emoji log.
        /// </returns>
        Task<long> CreateAsync(EmojiCreationData data);

        /// <summary>
        /// Creates multiple new emoji logs within the repository.
        /// </summary>
        /// <param name="data">The data for the emoji logs to be created.</param>
        /// <param name="count">The number of new logs to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task CreateMultipleAsync(EmojiCreationData data, int count);

        /// <summary>
        /// Deletes emoji logs within the repository.
        /// </summary>
        /// <param name="criteria">The criteria for the emoji to be deleted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task DeleteAsync(EmojiSearchCriteria criteria);

        /// <summary>
        /// Returns the number of times emoji matching the specified criteria occurred.
        /// </summary>
        /// <param name="criteria">The criteria for the emoji to be counted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a dictionary of emoji and their counts.
        /// </returns>
        Task<IReadOnlyDictionary<EphemeralEmoji, int>> GetCountsAsync(EmojiSearchCriteria criteria);

        /// <summary>
        /// Searches the emoji logs for emoji records matching the supplied criteria.
        /// </summary>
        /// <param name="criteria">The criteria with which to filter the emoji returned.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of emoji meeting the supplied criteria.
        /// </returns>
        Task<IReadOnlyCollection<EmojiSummary>> SearchSummariesAsync(EmojiSearchCriteria criteria);

        /// <summary>
        /// Retrieves statistics for a single emoji.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to retrieve statistics from.</param>
        /// <param name="emoji">The emoji to retrieve statistics for.</param>
        /// <param name="dateFilter">How far in the past to search for statistics.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing statistical information about the emoji.
        /// </returns>
        Task<SingleEmojiUsageStatistics> GetEmojiStatsAsync(ulong guildId, EphemeralEmoji emoji, TimeSpan? dateFilter = null);

        /// <summary>
        /// Retrieves statistics for emojis within a guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to retrieve statistics from.</param>
        /// <param name="sortDirection">The sort direction, determining whether to retrieve results from the top (ascending) or bottom (descending).</param>
        /// <param name="recordLimit">How many statistical records to retrieve.</param>
        /// <param name="dateFilter">How far in the past to search for statistics.</param>
        /// <param name="userId">The user to retrieve statistics for, if any.</param>
        /// <param name="emojiIds">The emojis to limit the results to, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing a collection of statistical information about the emoji in a guild.
        /// </returns>
        Task<IReadOnlyCollection<EmojiUsageStatistics>> GetEmojiStatsAsync(
            ulong guildId, SortDirection sortDirection, int recordLimit, TimeSpan? dateFilter = null, ulong? userId = null, IEnumerable<ulong>? emojiIds = null);

        /// <summary>
        /// Retrieves statistics about a guild's emoji usage.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild to retrieve statistics from.</param>
        /// <param name="emojiIds">The emojis to limit the results to, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// containing statistical information about a guild's emoji usage.
        /// </returns>
        Task<GuildEmojiStats> GetGuildStatsAsync(ulong guildId, ulong? userId = null, IEnumerable<ulong>? emojiIds = null);
    }

    /// <inheritdoc />
    public sealed class EmojiRepository : RepositoryBase, IEmojiRepository
    {
        /// <summary>
        /// Creates a new <see cref="EmojiRepository"/> with the injected dependencies
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public EmojiRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginMaintainTransactionAsync()
            => _maintainTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(EmojiCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            ModixContext.Set<EmojiEntity>().Add(entity);
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task CreateMultipleAsync(EmojiCreationData data, int count)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (count <= 0)
                return;

            var now = DateTimeOffset.UtcNow;
            var entities = Enumerable.Range(0, count).Select(_ => data.ToEntity(now));

            await ModixContext.Set<EmojiEntity>().AddRangeAsync(entities);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var entities = ModixContext.Set<EmojiEntity>().FilterBy(criteria);

            ModixContext.RemoveRange(entities);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<EphemeralEmoji, int>> GetCountsAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var emoji = await ModixContext.Set<EmojiEntity>().AsNoTracking()
                .FilterBy(criteria)
                .GroupBy(x => new
                {
                    x.EmojiId,
                    EmojiName = x.EmojiId == null
                        ? x.EmojiName
                        : null
                },
                x => new { x.EmojiId, x.EmojiName, x.IsAnimated, x.Timestamp })
                .ToArrayAsync();

            var counts = emoji.ToDictionary(
                x =>
                {
                    var mostRecentEmoji = x.OrderByDescending(y => y.Timestamp).First();
                    return EphemeralEmoji.FromRawData(mostRecentEmoji.EmojiName, mostRecentEmoji.EmojiId, mostRecentEmoji.IsAnimated);
                },
                x => x.Count());

            return counts;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<EmojiSummary>> SearchSummariesAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var emoji = await ModixContext.Set<EmojiEntity>().AsNoTracking()
                .FilterBy(criteria)
                .AsExpandable()
                .Select(EmojiSummary.FromEntityProjection)
                .ToArrayAsync();

            return emoji;
        }

        /// <inheritdoc />
        public async Task<SingleEmojiUsageStatistics> GetEmojiStatsAsync(ulong guildId, EphemeralEmoji emoji, TimeSpan? dateFilter = null)
        {
            var query = GetQuery();
            var parameters = GetParameters();

            var stats = await ModixContext.Database
                .SqlQueryRaw<SingleEmojiStatsDto>(query, parameters)
                .FirstOrDefaultAsync();

            return SingleEmojiUsageStatistics.FromDto(stats ?? new SingleEmojiStatsDto());

            NpgsqlParameter[] GetParameters() =>
                new[]
                {
                    new NpgsqlParameter(":GuildId", NpgsqlDbType.Bigint)
                    {
                        Value = unchecked((long)guildId),
                    },
                    emoji.Id is null
                        ? new NpgsqlParameter(":EmojiName", NpgsqlDbType.Text)
                        {
                            Value = emoji.Name,
                        }
                        : new NpgsqlParameter(":EmojiId", NpgsqlDbType.Bigint)
                        {
                            Value = unchecked((long)emoji.Id.Value),
                        },
                    new NpgsqlParameter(":StartTimestamp", NpgsqlDbType.TimestampTz)
                    {
                        Value = dateFilter is null
                            ? DateTimeOffset.MinValue
                            : DateTimeOffset.UtcNow - dateFilter
                    }
                };

            string GetQuery()
                => $@"
                    with user_stats as (
                        select ""UserId"" as ""TopUserId"", count(*) as ""TopUserUses""
                        from ""Emoji""
                        where ""GuildId"" = :GuildId
                            and ""Timestamp"" >= :StartTimestamp
                            and { (emoji.Id is null ? @"""EmojiName"" = :EmojiName" : @"""EmojiId"" = :EmojiId") }
                        group by ""UserId""
                        order by count(*) desc
                        limit 1
                    ),
                    stats as (
                        select ""EmojiId"", ""EmojiName"", ""IsAnimated"", count(*) as ""Uses"", row_number() over (order by count(*) desc) as ""Rank""
                        from ""Emoji""
                        where ""GuildId"" = :GuildId
                            and ""Timestamp"" >= :StartTimestamp
                        group by ""EmojiId"", ""EmojiName"", ""IsAnimated""
                    )
                    select ""EmojiId"", ""EmojiName"", ""IsAnimated"", ""Uses"", ""Rank"", ""TopUserId"", ""TopUserUses""
                    from stats, user_stats
                    where { (emoji.Id is null ? @"""EmojiName"" = :EmojiName" : @"""EmojiId"" = :EmojiId") }";
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<EmojiUsageStatistics>> GetEmojiStatsAsync(
            ulong guildId, SortDirection sortDirection, int recordLimit, TimeSpan? dateFilter = null, ulong? userId = null, IEnumerable<ulong>? emojiIds = null)
        {
            var parameters = GetParameters();
            var query = GetQuery();

            var stats = await ModixContext.Database
                .SqlQueryRaw<EmojiStatsDto>(query, parameters)
                .ToArrayAsync();

            return stats.Select(x => EmojiUsageStatistics.FromDto(x ?? new EmojiStatsDto())).ToArray();

            NpgsqlParameter[] GetParameters()
            {
                var paramList = new List<NpgsqlParameter>(3)
                {
                    new NpgsqlParameter(":GuildId", NpgsqlDbType.Bigint)
                    {
                        Value = unchecked((long)guildId),
                    },
                    new NpgsqlParameter(":StartTimestamp", NpgsqlDbType.TimestampTz)
                    {
                        Value = dateFilter is null
                            ? DateTimeOffset.MinValue
                            : DateTimeOffset.UtcNow - dateFilter
                    }
                };

                if (!(userId is null))
                {
                    paramList.Add(new NpgsqlParameter(":UserId", NpgsqlDbType.Bigint)
                    {
                        Value = unchecked((long)userId),
                    });
                }

                if (!(emojiIds is null) && emojiIds.Any())
                {
                    paramList.Add(new NpgsqlParameter(":EmojiIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
                    {
                        Value = emojiIds.Select(x => unchecked((long)x)).ToArray(),
                    });
                }

                return paramList.ToArray();
            }

            string GetQuery()
                => $@"
                    with stats as (
                        select ""EmojiId"", ""EmojiName"", ""IsAnimated"", count(*) as ""Uses"", row_number() over (order by count(*) desc) as ""Rank""
                        from ""Emoji""
                        where ""GuildId"" = :GuildId
                            and ""Timestamp"" >= :StartTimestamp
                            {(userId is null ? string.Empty : @"and ""UserId"" = :UserId")}
                            {(emojiIds is null || !emojiIds.Any() ? string.Empty : @"and ""EmojiId"" = any(:EmojiIds)")}
                        group by ""EmojiId"", ""EmojiName"", ""IsAnimated""
                        order by ""Rank"" {(sortDirection == SortDirection.Ascending ? "asc" : "desc")}
                        limit {recordLimit}
                    )
                    select ""EmojiId"", ""EmojiName"", ""IsAnimated"", ""Uses"", ""Rank""
                    from stats
                    order by ""Rank"" asc";
        }

        /// <inheritdoc />
        public async Task<GuildEmojiStats> GetGuildStatsAsync(ulong guildId, ulong? userId = null, IEnumerable<ulong>? emojiIds = null)
        {
            var parameters = GetParameters();
            var query = GetQuery();

            var stats = await ModixContext.Database
                .SqlQueryRaw<GuildEmojiStats>(query, parameters)
                .FirstOrDefaultAsync();

            return stats ?? new();

            NpgsqlParameter[] GetParameters()
            {
                var paramList = new List<NpgsqlParameter>(3)
                {
                    new NpgsqlParameter(":GuildId", NpgsqlDbType.Bigint)
                    {
                        Value = unchecked((long)guildId),
                    },
                };

                if (!(emojiIds is null) && emojiIds.Any())
                {
                    paramList.Add(new NpgsqlParameter(":EmojiIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
                    {
                        Value = emojiIds.Select(x => unchecked((long)x)).ToArray(),
                    });
                }

                if(userId.HasValue)
                {
                    paramList.Add(new NpgsqlParameter(":UserId", NpgsqlDbType.Bigint)
                    {
                        Value = unchecked((long)userId),
                    });
                }

                return paramList.ToArray();
            }

            string GetQuery()
                => $@"
                    with stats as (
                        select count(distinct coalesce(cast(""EmojiId"" as text), ""EmojiName"")) as ""UniqueEmojis"", count(*) as ""TotalUses"", coalesce(min(""Timestamp""), now()) as ""OldestTimestamp""
                        from ""Emoji""
                        where ""GuildId"" = :GuildId
                        {(userId.HasValue ? @"and ""UserId"" = :UserId" : string.Empty)}
                        {(emojiIds is null || !emojiIds.Any() ? string.Empty : @"and ""EmojiId"" = any(:EmojiIds)")}
                        limit 1
                    )
                    select ""UniqueEmojis"", ""TotalUses"", ""OldestTimestamp""
                    from stats";
        }

        private static readonly RepositoryTransactionFactory _maintainTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
