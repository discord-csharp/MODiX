using System.Collections.Generic;

namespace System.Linq
{
    public static class EnumerableEx
    {
        public static IEnumerable<T> From<T>(
            T item)
        {
            yield return item;
        }
    }
}
