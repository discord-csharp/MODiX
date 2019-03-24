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
        Task<IReadOnlyCollection<EmojiSummary>> GetEmojiSummaries(IEmote emote, ulong guildId, TimeSpan? timeSpan);

        IReadOnlyDictionary<EphemeralEmoji, int> GetCountFromSummary(IEnumerable<EmojiSummary> emojiSummaries);

        int GetTotalEmojiUseCount(IEnumerable<KeyValuePair<EphemeralEmoji, int>> emojis);

        DateTimeOffset GetOldestSummaryTimeStamp(IEnumerable<EmojiSummary> emojiSummaries);
    }

    public class EmojiStatsService : IEmojiStatsService
    {
        private readonly IEmojiRepository _emojiRepository;

        public EmojiStatsService(IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }


        public async Task<IReadOnlyCollection<EmojiSummary>> GetEmojiSummaries(IEmote emote, ulong guildId, TimeSpan? timeSpan)
        {
            var criteria = new EmojiSearchCriteria()
            {
                EmojiName = emote?.Name,
                GuildId = guildId,
            };

            if (timeSpan.HasValue)
            {
                criteria.TimestampRange = new DateTimeOffsetRange()
                {
                    From = DateTimeOffset.UtcNow - timeSpan.Value,
                    To = DateTimeOffset.UtcNow
                };
            }

            return await _emojiRepository.SearchSummariesAsync(criteria);
        }

        public IReadOnlyDictionary<EphemeralEmoji, int> GetCountFromSummary(IEnumerable<EmojiSummary> emojiSummaries)
        {
            return emojiSummaries
                .GroupBy(x => new { x.Emoji, x.Emoji.Id, Name = x.Emoji.Id == null ? x.Emoji.Name : null })
                .ToDictionary(x => x.Key.Emoji,
                              x => x.Count(),
                              new EphemeralEmoji.EqualityComparer());
        }

        public int GetTotalEmojiUseCount(IEnumerable<KeyValuePair<EphemeralEmoji,int>> emojis)
        {
            return emojis.Sum(x => x.Value);
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
