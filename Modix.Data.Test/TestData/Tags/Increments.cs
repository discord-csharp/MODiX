using System;
using System.Collections.Generic;

using Modix.Data.Test.TestData.Tags.TryIncrementAsync;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Increments
    {
        public static readonly IEnumerable<ExceptionIncrementTestData> ExceptionIncrements
            = new[]
            {
                new ExceptionIncrementTestData()
                {
                    TestName = "Null name",
                    GuildId = 1,
                    TagName = null,
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionIncrementTestData()
                {
                    TestName = "Empty name",
                    GuildId = 1,
                    TagName = "",
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionIncrementTestData()
                {
                    TestName = "Whitespace name",
                    GuildId = 1,
                    TagName = " \r\n\t",
                    ExceptionType = typeof(ArgumentException),
                },
            };

        public static readonly IEnumerable<NonexistentIncrementTestData> NonexistentIncrements
            = new[]
            {
                new NonexistentIncrementTestData()
                {
                    TestName = "Nonexistent name",
                    GuildId = 1,
                    TagName = "NONEXISTENTNAME",
                },
                new NonexistentIncrementTestData()
                {
                    TestName = "Wrong guild",
                    GuildId = 42,
                    TagName = "created",
                },
            };
        
        public static readonly IEnumerable<ValidIncrementTestData> ValidIncrements
            = new[]
            {
                new ValidIncrementTestData()
                {
                    TestName = "Increment from 0",
                    GuildId = 1,
                    TagName = "created",
                    ResultantUses = 1,
                },
                new ValidIncrementTestData()
                {
                    TestName = "Increment from 64",
                    GuildId = 2,
                    TagName = "sametagacrossguilds",
                    ResultantUses = 65,
                },
            };
    }
}
