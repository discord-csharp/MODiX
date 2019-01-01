using System;
using System.Collections.Generic;

using Modix.Data.Test.TestData.Tags.TryDeleteAsync;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Deletions
    {
        public static readonly IEnumerable<ExceptionDeleteTestData> ExceptionDeletions
            = new[]
            {
                new ExceptionDeleteTestData()
                {
                    TestName = "Null name",
                    GuildId = 1,
                    TagName = null,
                    DeletedByUserId = 1,
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionDeleteTestData()
                {
                    TestName = "Empty name",
                    GuildId = 1,
                    TagName = "",
                    DeletedByUserId = 1,
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionDeleteTestData()
                {
                    TestName = "Whitespace name",
                    GuildId = 1,
                    TagName = " \r\n\t",
                    DeletedByUserId = 1,
                    ExceptionType = typeof(ArgumentException),
                },
            };

        public static readonly IEnumerable<NonexistentDeleteTestData> NonexistentDeletions
            = new[]
            {
                new NonexistentDeleteTestData()
                {
                    TestName = "Nonexistent name",
                    GuildId = 1,
                    TagName = "NONEXISTENTNAME",
                    DeletedByUserId = 1,
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Wrong guild",
                    GuildId = 42,
                    TagName = "created",
                    DeletedByUserId = 1,
                },
                new NonexistentDeleteTestData()
                {
                    TestName = "Already deleted",
                    GuildId = 1,
                    TagName = "deleted",
                    DeletedByUserId = 1,
                },
            };
        
        public static readonly IEnumerable<ValidDeleteTestData> ValidDeletions
            = new[]
            {
                new ValidDeleteTestData()
                {
                    TestName = "Delete from guild 1",
                    GuildId = 1,
                    TagName = "created",
                    DeletedByUserId = 1,
                },
                new ValidDeleteTestData()
                {
                    TestName = "Delete from guild 2",
                    GuildId = 2,
                    TagName = "sametagacrossguilds",
                    DeletedByUserId = 3,
                },
            };
    }
}
