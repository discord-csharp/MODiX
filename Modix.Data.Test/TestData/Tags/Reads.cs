using System;
using System.Collections.Generic;

using Modix.Data.Test.TestData.Tags.ReadSummaryAsync;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Reads
    {
        public static readonly IEnumerable<ExceptionReadTestData> ExceptionReads
            = new[]
            {
                new ExceptionReadTestData()
                {
                    TestName = "Null name",
                    GuildId = 1,
                    TagName = null,
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionReadTestData()
                {
                    TestName = "Empty name",
                    GuildId = 1,
                    TagName = "",
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionReadTestData()
                {
                    TestName = "Whitespace name",
                    GuildId = 1,
                    TagName = " \r\n\t",
                    ExceptionType = typeof(ArgumentException),
                },
            };

        public static readonly IEnumerable<NonexistentReadTestData> NonexistentReads
            = new[]
            {
                new NonexistentReadTestData()
                {
                    TestName = "Nonexistent name",
                    GuildId = 1,
                    TagName = "NONEXISTENTNAME",
                },
                new NonexistentReadTestData()
                {
                    TestName = "Wrong guild",
                    GuildId = 42,
                    TagName = "created",
                },
            };

        public static readonly IEnumerable<ValidReadTestData> ValidReads
            = new[]
            {
                new ValidReadTestData()
                {
                    TestName = "Valid criteria",
                    GuildId = 1,
                    TagName = "created",
                    ResultId = 1,
                },
                new ValidReadTestData()
                {
                    TestName = "Guild 1",
                    GuildId = 1,
                    TagName = "sametagacrossguilds",
                    ResultId = 7,
                },
                new ValidReadTestData()
                {
                    TestName = "Guild 2",
                    GuildId = 2,
                    TagName = "sametagacrossguilds",
                    ResultId = 8,
                },
            };
    }
}
