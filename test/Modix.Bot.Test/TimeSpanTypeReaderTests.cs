using System;
using NUnit.Framework;
using Shouldly;

namespace Modix.Bot.Test
{
    public class TimeSpanTypeReaderTests
    {
        internal static readonly string[] InvalidInputs
            = new[]
            {
                "",
                "2",
                "z",
                "24z",
                "-1h",
                "2h-1m",
                "1hour",
                "h20",
                " 12h",
                "12h ",
                "test",
            };

        [TestCaseSource(nameof(InvalidInputs))]
        public void TryParseTimeSpan_GivenInvalidInput_ReturnsFalse(string input)
        {
            var uut = new TimeSpanTypeReader();

            var succeeded = uut.TryParseTimeSpan(input, out _);

            succeeded.ShouldBeFalse();
        }

        internal static readonly object[] ValidInputs
            = new[]
            {
                new object[] { "0s", TimeSpan.Zero },
                new object[] { "1s", TimeSpan.FromSeconds(1) },
                new object[] { "60s", TimeSpan.FromSeconds(60) },
                new object[] { "61s", TimeSpan.FromSeconds(61) },

                new object[] { "0m", TimeSpan.Zero },
                new object[] { "1m", TimeSpan.FromMinutes(1) },
                new object[] { "60m", TimeSpan.FromMinutes(60) },
                new object[] { "61m", TimeSpan.FromMinutes(61) },

                new object[] { "0h", TimeSpan.Zero },
                new object[] { "1h", TimeSpan.FromHours(1) },
                new object[] { "24h", TimeSpan.FromHours(24) },
                new object[] { "25h", TimeSpan.FromHours(25) },

                new object[] { "0d", TimeSpan.Zero },
                new object[] { "1d", TimeSpan.FromDays(1) },
                new object[] { "31d", TimeSpan.FromDays(31) },
                new object[] { "32d", TimeSpan.FromDays(32) },

                new object[] { "0w", TimeSpan.Zero },
                new object[] { "1w", TimeSpan.FromDays(7) },
                new object[] { "4w", TimeSpan.FromDays(28) },
                new object[] { "100w", TimeSpan.FromDays(700) },

                new object[] { "0w0d0h0m0s", TimeSpan.Zero },
                new object[] { "1w1d1h1m1s", TimeSpan.FromDays(7) + TimeSpan.FromDays(1) + TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1) },
                new object[] { "4w31d24h60m60s", TimeSpan.FromDays(28) + TimeSpan.FromDays(31) + TimeSpan.FromHours(24) + TimeSpan.FromMinutes(60) + TimeSpan.FromSeconds(60) },
                new object[] { "100w32d25h61m61s", TimeSpan.FromDays(700) + TimeSpan.FromDays(32) + TimeSpan.FromHours(25) + TimeSpan.FromMinutes(61) + TimeSpan.FromSeconds(61) },
            };

        [TestCaseSource(nameof(ValidInputs))]
        public void TryParseTimeSpan_GivenValidInput_SuccessfullyParses(string input, TimeSpan expected)
        {
            var uut = new TimeSpanTypeReader();

            var succeeded = uut.TryParseTimeSpan(input, out var actual);

            succeeded.ShouldBeTrue();
            actual.ShouldBe(expected);
        }
    }
}
