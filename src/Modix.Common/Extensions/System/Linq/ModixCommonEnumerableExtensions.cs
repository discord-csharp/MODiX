using System.Collections.Generic;

namespace System.Linq
{
    public static class ModixCommonEnumerableExtensions
    {
        public static double? AverageOrNull(
            this IEnumerable<long> values)
        {
            var total = 0L;
            var count = 0;
            foreach(var value in values)
            {
                total += value;
                ++count;
            }

            return (count > 0)
                ? (total / count)
                : null as long?;
        }
    }
}
