using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(
            this IEnumerable<T> sequence,
            Action<T> action)
        {
            foreach (var item in sequence)
                action.Invoke(item);
        }
    }
}
