using System;
using System.Collections.Generic;

using Modix.Data.Test.TestData.Tags.SearchSummariesAsync;

namespace Modix.Data.Test.TestData.Tags
{
    internal static class Searches
    {
        public static readonly IEnumerable<ExceptionSearchTestData> ExceptionSearches
            = new[]
            {
                new ExceptionSearchTestData()
                {
                    TestName = "Null query",
                    GuildId = 1,
                    Query = null,
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionSearchTestData()
                {
                    TestName = "Empty query",
                    GuildId = 1,
                    Query = "",
                    ExceptionType = typeof(ArgumentException),
                },
                new ExceptionSearchTestData()
                {
                    TestName = "Whitespace query",
                    GuildId = 1,
                    Query = " \r\n\t",
                    ExceptionType = typeof(ArgumentException),
                },
            };

        public static readonly IEnumerable<NonexistentSearchTestData> NonexistentSearches
            = new[]
            {
                new NonexistentSearchTestData()
                {
                    TestName = "Nonexistent query",
                    GuildId = 1,
                    Query = "NONEXISTENTQUERY",
                },
                new NonexistentSearchTestData()
                {
                    TestName = "Wrong guild",
                    GuildId = 42,
                    Query = "created",
                },
            };

        public static readonly IEnumerable<ValidSearchTestData> ValidSearches
            = new[]
            {
                new ValidSearchTestData()
                {
                    TestName = "Single result",
                    GuildId = 2,
                    Query = "otherguildcreated",
                    ResultIds = new[] { 3L },
                },
                new ValidSearchTestData()
                {
                    TestName = "Two results",
                    GuildId = 1,
                    Query = "created",
                    ResultIds = new[] { 1L, 6L },
                },
                new ValidSearchTestData()
                {
                    TestName = "Single result guild 1",
                    GuildId = 1,
                    Query = "sametagacrossguilds",
                    ResultIds = new[] { 7L },
                },
                new ValidSearchTestData()
                {
                    TestName = "Single result guild 2",
                    GuildId = 2,
                    Query = "sametagacrossguilds",
                    ResultIds = new[] { 8L },
                },
            };
    }
}
