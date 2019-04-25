using System;
using System.Collections.Generic;
using Modix.Data.Models.Emoji;
using Modix.Data.Test.TestData.Emoji.SingleEmojiStats;

namespace Modix.Data.Test.TestData.Emoji
{
    internal static class SingleEmoji
    {
        public static readonly IEnumerable<ExceptionReadTestData> Exceptions
            = new[]
            {
                new ExceptionReadTestData()
                {
                    TestName = "Null emoji",
                    Emoji = null,
                    ExceptionType = typeof(ArgumentNullException),
                },
            };

        public static readonly IEnumerable<NonexistentReadTestData> Nonexistent
            = new[]
            {
                new NonexistentReadTestData()
                {
                    TestName = "Nonexistent guild",
                    GuildId = 0,
                    Emoji = EphemeralEmoji.FromRawData("emoji1", 1),
                    DateFilter = null,
                },
                new NonexistentReadTestData()
                {
                    TestName = "Nonexistent emoji",
                    GuildId = 1,
                    Emoji = new EphemeralEmoji(),
                    DateFilter = null,
                },
                new NonexistentReadTestData()
                {
                    TestName = "Non-historical timestamp",
                    GuildId = 1,
                    Emoji = EphemeralEmoji.FromRawData("emoji1", 1),
                    DateFilter = TimeSpan.Zero
                },
            };

        public static readonly IEnumerable<ValidReadTestData> Valid
            = new[]
            {
                new ValidReadTestData()
                {
                    TestName = "Guild 1, emoji 1, all time",
                    GuildId = 1,
                    Emoji = EphemeralEmoji.FromRawData("emoji1", 1),
                    DateFilter = null,
                },
            };
    }
}
