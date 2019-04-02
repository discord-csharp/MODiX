using System;

namespace Modix.Data.Models.Emoji
{
    public class GuildEmojiStats
    {
        public int UniqueEmojis { get; set; }

        public int TotalUses { get; set; }

        public DateTimeOffset OldestTimestamp { get; set; }
    }
}
