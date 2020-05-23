#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Discord;

using NUnit.Framework;

using Shouldly;

namespace Modix.Services.Test.Extensions.Discord
{
    [TestFixture]
    public class StringExtensionsTests
    {
        #region EnumerateLongTextAsFieldBuilders() Tests

        [Test]
        public void EnumerateLongTextAsFieldBuilders_TextIsEmpty_ThrowsException()
        {
            var result = Should.Throw<ArgumentException>(() =>
            {
                _ = string.Empty.EnumerateLongTextAsFieldBuilders("Empty Text Field")
                    .ToArray();
            });

            result.ParamName.ShouldBe("text");
        }

        [Test]
        public void EnumerateLongTextAsFieldBuilders_FieldNameIsEmpty_ThrowsException()
        {
            var result = Should.Throw<ArgumentException>(() =>
            {
                _ = "Field Value".EnumerateLongTextAsFieldBuilders(string.Empty)
                    .ToArray();
            });

            result.ParamName.ShouldBe("fieldName");
        }

        public static readonly ImmutableArray<TestCaseData> EnumerateLongTextAsFieldBuilders_TestCaseData
            = ImmutableArray.Create(
                /*                  fieldName,              text,                   fields  (name,                  value)                      */
                new TestCaseData(   "5",                    "6",                    new[] { ("5",                   "6")                    }   ).SetName("{m}(Unique Values 3)"),
                new TestCaseData(   "1",                    "2",                    new[] { ("1",                   "2")                    }   ).SetName("{m}(Unique Values 1)"),
                new TestCaseData(   "3",                    "4",                    new[] { ("3",                   "4")                    }   ).SetName("{m}(Unique Values 2)"),
                new TestCaseData(   "Long Text Field 1",    new string('A', 1024),  new[] { ("Long Text Field 1",   new string('A', 1024))  }   ).SetName("{m}(Long text, 1 Field, Upper Limit)"),
                new TestCaseData(   "Long Text Field 2",    new string('B', 2000),  new[] { ("Long Text Field 2",   new string('B', 1024)),
                                                                                            ("(continued)",         new string('B', 976))   }   ).SetName("{m}(Long text, 2 Fields)"),
                new TestCaseData(   "Long Text Field 3",    new string('C', 2048),  new[] { ("Long Text Field 3",   new string('C', 1024)),
                                                                                            ("(continued)",         new string('C', 1024))  }   ).SetName("{m}(Long text, 2 Fields, Upper Limit)"),
                new TestCaseData(   "Long Text Field 4",    new string('D', 3000),  new[] { ("Long Text Field 4",   new string('D', 1024)),
                                                                                            ("(continued)",         new string('D', 1024)),
                                                                                            ("(continued)",         new string('D', 952))   }   ).SetName("{m}(Long text, 3 Fields)"));

        [TestCaseSource(nameof(EnumerateLongTextAsFieldBuilders_TestCaseData))]
        public void EnumerateLongTextAsFieldBuilders_Otherwise_ResultIsExpected(
            string fieldName,
            string text,
            IReadOnlyList<(string name, string value)> fields)
        {
            var results = text.EnumerateLongTextAsFieldBuilders(fieldName)
                .ToArray();

            results.Length.ShouldBe(fields.Count);
            foreach(var (result, field) in Enumerable.Zip(results, fields))
            {
                result.Name.ShouldBe(field.name);
                result.Value.ShouldBe(field.value);
            }
        }

        #endregion EnumerateLongTextAsFieldBuilders() Tests
    }
}
