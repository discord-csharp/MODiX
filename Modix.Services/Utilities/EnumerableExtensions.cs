using System;
using System.Collections.Generic;

namespace Modix.Services.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T Value, int Index)> AsIndexable<T>(this IEnumerable<T> source)
        {
            var index = 0;

            foreach (var item in source)
            {
                yield return (item, index);
                index++;
            }
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var possibleTail = new Queue<T>();

            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    // We found an item that doesn't match the predicate, so
                    // anything we weren't sure about is now safe to return.
                    while (possibleTail.TryDequeue(out var queuedItem))
                    {
                        yield return queuedItem;
                    }

                    yield return item;
                }
                else
                {
                    possibleTail.Enqueue(item);
                }
            }
        }

        public static IEnumerable<(TFirst First, TSecond Second)> ZipOrDefault<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
            using var e1 = first.GetEnumerator();
            using var e2 = second.GetEnumerator();

            while (true)
            {
                var e1Moved = e1.MoveNext();
                var e2Moved = e2.MoveNext();

                if (!e1Moved && !e2Moved)
                    break;

                yield return
                (
                    e1Moved ? e1.Current : default,
                    e2Moved ? e2.Current : default
                );
            }
        }
    }
}
