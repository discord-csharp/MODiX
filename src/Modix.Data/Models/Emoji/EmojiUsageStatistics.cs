namespace Modix.Data.Models.Emoji
{
    public class EmojiUsageStatistics
    {
        public EphemeralEmoji Emoji { get; set; } = null!;

        public int Rank { get; set; }

        public int Uses { get; set; }

        internal static EmojiUsageStatistics FromDto(EmojiStatsDto emojiStatsDto)
            => new EmojiUsageStatistics()
            {
                Emoji = EphemeralEmoji.FromRawData(emojiStatsDto.EmojiName, emojiStatsDto.EmojiId, emojiStatsDto.IsAnimated),
                Rank = emojiStatsDto.Rank,
                Uses = emojiStatsDto.Uses,
            };
    }
}
