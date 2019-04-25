using System;
using System.Collections.Generic;
using System.Linq;
using Modix.Data.Models;
using Modix.Data.Models.Emoji;
using Modix.Data.Test.TestData.Emoji.DeleteAsync;

namespace Modix.Data.Test.TestData.Emoji
{
    internal static class Deletions
    {
        public static readonly IEnumerable<ExceptionDeleteTestData> ExceptionDeletions
            = new[]
            {
                new ExceptionDeleteTestData()
                {
                    TestName = "Null criteria",
                    Criteria = null,
                    ExceptionType = typeof(ArgumentNullException),
                },
            };

        public static readonly IEnumerable<NonexistentDeleteTestData> NonexistentDeletions
            = new[]
            {
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent guild",
                    Criteria = new EmojiSearchCriteria()
                    {
                        GuildId = 0,
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent channel",
                    Criteria = new EmojiSearchCriteria()
                    {
                        ChannelId = 0,
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent message",
                    Criteria = new EmojiSearchCriteria()
                    {
                        MessageId = 0,
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent user",
                    Criteria = new EmojiSearchCriteria()
                    {
                        UserId = 0,
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent emoji ID",
                    Criteria = new EmojiSearchCriteria()
                    {
                        EmojiId = 0,
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent emoji name",
                    Criteria = new EmojiSearchCriteria()
                    {
                        EmojiName = "! DOES_NOT_EXIST !",
                    },
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Future timestamp",
                    Criteria = new EmojiSearchCriteria()
                    {
                        TimestampRange = new DateTimeOffsetRange()
                        {
                            From = DateTimeOffset.UtcNow + TimeSpan.FromDays(1),
                            To = DateTimeOffset.UtcNow + TimeSpan.FromDays(2),
                        },
                    },
                },
            };

        public static readonly IEnumerable<ValidDeleteTestData> ValidDeletions
            = new[]
            {
                new ValidDeleteTestData()
                {
                    TestName = "Valid guild",
                    Criteria = new EmojiSearchCriteria()
                    {
                        GuildId = 1,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.GuildId == 1),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid channel",
                    Criteria = new EmojiSearchCriteria()
                    {
                        ChannelId = 1,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.ChannelId == 1),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid message",
                    Criteria = new EmojiSearchCriteria()
                    {
                        MessageId = 1,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.MessageId == 1),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid user 1",
                    Criteria = new EmojiSearchCriteria()
                    {
                        UserId = 1,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.UserId == 1),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid user 2",
                    Criteria = new EmojiSearchCriteria()
                    {
                        UserId = 2,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.UserId == 2),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid user 3",
                    Criteria = new EmojiSearchCriteria()
                    {
                        UserId = 3,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.UserId == 3),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid emoji ID",
                    Criteria = new EmojiSearchCriteria()
                    {
                        EmojiId = 1,
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.EmojiId == 1),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid emoji name",
                    Criteria = new EmojiSearchCriteria()
                    {
                        EmojiName = "emoji1",
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.EmojiName == "emoji1"),
                },
                new ValidDeleteTestData()
                {
                    TestName = "Valid timestamp",
                    Criteria = new EmojiSearchCriteria()
                    {
                        TimestampRange = new DateTimeOffsetRange()
                        {
                            From = DateTimeOffset.UtcNow - TimeSpan.FromDays(10),
                        },
                    },
                    DeletedCount = Emoji.Entities.Count(x => x.Timestamp >= DateTimeOffset.UtcNow - TimeSpan.FromDays(10)),
                },
                new ValidDeleteTestData()
                {
                    TestName = "All records",
                    Criteria = new EmojiSearchCriteria(),
                    DeletedCount = Emoji.Entities.Count(),
                },
            };
    }
}
