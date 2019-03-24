using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;

namespace Modix.Services.EmojiStats
{
    public interface IEmojiStatsService
    {
        Task<IReadOnlyCollection<EmojiSummary>> GetEmojiSummaries(IEmote emote, ulong guildId, bool monthly);

        IReadOnlyDictionary<EphemeralEmoji, int> GetCountFromSummary(IEnumerable<EmojiSummary> emojiSummaries);

        int AggregateUsage(IEnumerable<KeyValuePair<EphemeralEmoji, int>> emojis);

        DateTimeOffset GetOldestSummaryTimeStamp(IEnumerable<EmojiSummary> emojiSummaries);
    }

    public class EmojiStatsService : IEmojiStatsService
    {
        private readonly IEmojiRepository _emojiRepository;

        public EmojiStatsService(IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }


        public async Task<IReadOnlyCollection<EmojiSummary>> GetEmojiSummaries(IEmote emote, ulong guildId, bool monthly)
        {
            var criteria = new EmojiSearchCriteria()
            {
                EmojiName = emote?.Name,
                GuildId = guildId,
            };

            if (monthly)
            {
                criteria.TimestampRange = new DateTimeOffsetRange()
                {
                    From = DateTimeOffset.UtcNow - TimeSpan.FromDays(30),
                    To = DateTimeOffset.UtcNow
                };
            }

            return await _emojiRepository.SearchSummariesAsync(criteria);
        }

        public IReadOnlyDictionary<EphemeralEmoji, int> GetCountFromSummary(IEnumerable<EmojiSummary> emojiSummaries)
        {
            return emojiSummaries
                .GroupBy(x => new { x.Emoji.Id, x.Emoji.Name })
                .ToDictionary(x => EphemeralEmoji.FromRawData(x.Key.Name, x.Key.Id),
                              x => x.Count(),
                              new EphemeralEmoji.EqualityComparer());
        }

        public int AggregateUsage(IEnumerable<KeyValuePair<EphemeralEmoji,int>> emojis)
        {
            return emojis
                .Select(x => x.Value)
                .Aggregate((i, j) => i + j);
        }

        public DateTimeOffset GetOldestSummaryTimeStamp(IEnumerable<EmojiSummary> emojiSummaries)
        {
            return emojiSummaries
                .OrderByDescending(x => x.Timestamp)
                .First()
                .Timestamp;
        }
    }
}
