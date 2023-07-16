using System;
using System.Collections.Generic;

namespace Shouldly
{
    public static class EnumerableAssertions
    {
        public static void EachShould<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(sequence);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in sequence)
                action.Invoke(item);
        }

        public static void ShouldBeSetEqualTo<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
            => actual.ShouldBe(expected, ignoreOrder: true);
    }
}
