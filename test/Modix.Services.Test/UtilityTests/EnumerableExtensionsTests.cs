using Modix.Services.Utilities;

using NUnit.Framework;

using Shouldly;

namespace Modix.Services.Test.UtilityTests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void SkipLast_GivenAlwaysFalsePredicate_ReturnsEntireEnumerable()
        {
            var enumerable = new[] { 1, 2, 3, 4, 5 };

            var result = enumerable.SkipLast(_ => false);

            result.ShouldBe(enumerable);
        }

        [Test]
        public void SkipLast_GivenAlwaysTruePredicate_ReturnsEmptyEnumerable()
        {
            var enumerable = new[] { 1, 2, 3, 4, 5 };

            var result = enumerable.SkipLast(_ => true);

            result.ShouldBeEmpty();
        }

        [Test]
        public void SkipLast_GivenMatchingPredicate_ReturnsEnumerableWithoutTrailingMatches()
        {
            var enumerable = new[] { 1, 2, 3, 4, 5, 2, 4, 6 };

            var result = enumerable.SkipLast(x => x % 2 == 0);

            result.ShouldBe(new[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        public void SkipLast_GivenNonmatchingPredicate_ReturnsEntireEnumerable()
        {
            var enumerable = new[] { 1, 2, 3, 4, 5, 2, 4, 6 };

            var result = enumerable.SkipLast(x => x % 2 != 0);

            result.ShouldBe(enumerable);
        }
    }
}
