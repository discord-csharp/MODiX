using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;

namespace Modix.Services.EmojiStats
{
    /// <summary>
    /// Defines a service for querying information related to emoji usage.
    /// </summary>
    public interface IEmojiStatsService
    {
        /// <summary>
        /// Retrieves a collection of emoji statistics for the supplied guild.
        /// </summary>
        /// <param name="guildId">The guild for which to search for emoji records.</param>
        /// <param name="sortDirection">Indicates whether to retireve statistics from the top (ascending) or bottom (descending).</param>
        /// <param name="recordCount">The number of statistics records to retrieve.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing a collection of emoji statistics for the supplied guild.
        /// </returns>
        Task<IReadOnlyCollection<EmojiUsageStatistics>> GetEmojiStatsAsync(ulong guildId, SortDirection sortDirection, int recordCount, TimeSpan? dateFilter = null);

        Task<GuildEmojiStats> GetGuildStatsAsync(ulong guildId);

        /// <summary>
        /// Aggregates the data from a sequence of emoji log records to determine how many times each emoji was used.
        /// </summary>
        /// <param name="emojiSummaries">The sequence of emoji log records for which to aggregate the counts.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emojiSummaries"/>.</exception>
        /// <returns>
        /// A dictionary containing an entry for each emoji that appears in the sequence with a count of how many times that emoji was used.
        /// </returns>
        IReadOnlyDictionary<EphemeralEmoji, int> GetCountsFromSummaries(IEnumerable<EmojiSummary> emojiSummaries);

        /// <summary>
        /// Determines the total number of times emoji were used, based on a sequence of emoji log records and their individual counts.
        /// </summary>
        /// <param name="emojis">The sequence of emoji log records with counts to sum.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emojis"/>.</exception>
        /// <returns>
        /// The total number of emoji used.
        /// </returns>
        int GetTotalEmojiUseCount(IEnumerable<KeyValuePair<EphemeralEmoji, int>> emojis);

        /// <summary>
        /// Determines the oldest timestamp present within a sequence of emoji log records.
        /// </summary>
        /// <param name="emojiSummaries">The sequence of emoji log records to query for oldest timestamp.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emojiSummaries"/>.</exception>
        /// <returns>
        /// The oldest timestamp of any emoji log record in the supplied sequence.
        /// </returns>
        DateTimeOffset GetOldestSummaryTimestamp(IEnumerable<EmojiSummary> emojiSummaries);
    }

    /// <inheritdoc />
    internal class EmojiStatsService : IEmojiStatsService
    {
        private readonly IEmojiRepository _emojiRepository;

        /// <summary>
        /// Constructs a new <see cref="EmojiStatsService"/> with the given dependencies.
        /// </summary>
        public EmojiStatsService(IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<EmojiUsageStatistics>> GetEmojiStatsAsync(ulong guildId, SortDirection sortDirection, int recordCount, TimeSpan? dateFilter = null)
            => await _emojiRepository.GetEmojiStatsAsync(guildId, sortDirection, recordCount, dateFilter);

        public async Task<GuildEmojiStats> GetGuildStatsAsync(ulong guildId)
            => await _emojiRepository.GetGuildStatsAsync(guildId);

        /// <inheritdoc />
        public IReadOnlyDictionary<EphemeralEmoji, int> GetCountsFromSummaries(IEnumerable<EmojiSummary> emojiSummaries)
        {
            if (emojiSummaries is null)
                throw new ArgumentNullException(nameof(emojiSummaries));

            return emojiSummaries
                .GroupBy(x =>
                (
                    x.Emoji.Id,
                    x.Emoji.Id is null
                        ? x.Emoji.Name
                        : null
                ),
                x => x)
                .ToDictionary(x => x.OrderByDescending(y => y.Timestamp).First().Emoji,
                              x => x.Count());
        }

        /// <inheritdoc />
        public int GetTotalEmojiUseCount(IEnumerable<KeyValuePair<EphemeralEmoji,int>> emojis)
        {
            if (emojis is null)
                throw new ArgumentNullException(nameof(emojis));

            return emojis.Sum(x => x.Value);
        }

        /// <inheritdoc />
        public DateTimeOffset GetOldestSummaryTimestamp(IEnumerable<EmojiSummary> emojiSummaries)
        {
            if (emojiSummaries is null)
                throw new ArgumentNullException(nameof(emojiSummaries));

            if (!emojiSummaries.Any())
                return DateTimeOffset.Now;

            return emojiSummaries
                .Min(x => x.Timestamp)
                .ToUniversalTime();
        }
    }
}
