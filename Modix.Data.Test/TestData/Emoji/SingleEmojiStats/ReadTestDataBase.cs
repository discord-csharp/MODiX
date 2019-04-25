using System;
using Modix.Data.Models.Emoji;

namespace Modix.Data.Test.TestData.Emoji.SingleEmojiStats
{
    internal abstract class ReadTestDataBase
    {
        public string TestName { get; set; }

        public ulong GuildId { get; set; }

        public EphemeralEmoji Emoji { get; set; }

        public TimeSpan? DateFilter { get; set; }
    }
}
