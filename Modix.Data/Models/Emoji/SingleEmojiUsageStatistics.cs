namespace Modix.Data.Models.Emoji
{
    public class SingleEmojiUsageStatistics
    {
        public EphemeralEmoji Emoji { get; set; } = null!;

        public int Rank { get; set; }

        public int Uses { get; set; }

        public ulong TopUserId { get; set; }

        public int TopUserUses { get; set; }

        internal static SingleEmojiUsageStatistics FromDto(SingleEmojiStatsDto emojiStatsDto)
            => new SingleEmojiUsageStatistics()
            {
                Emoji = EphemeralEmoji.FromRawData(emojiStatsDto.EmojiName, emojiStatsDto.EmojiId, emojiStatsDto.IsAnimated),
                Rank = emojiStatsDto.Rank,
                Uses = emojiStatsDto.Uses,
                TopUserId = emojiStatsDto.TopUserId,
                TopUserUses = emojiStatsDto.TopUserUses,
            };
    }
}
