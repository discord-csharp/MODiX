using System;
using System.Linq;
using System.Linq.Expressions;

namespace Modix.Data.ExpandableQueries
{
    public static class ExpandableExtensions
    {
        public static IQueryable<T> AsExpandable<T>(this IQueryable<T> query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (query is ExpandableQuery<T>)
                return query;

            return new ExpandableQueryProvider(query.Provider).CreateQuery<T>(query.Expression);
        }

        public static TOut Project<TIn, TOut>(this TIn input, Expression<Func<TIn, TOut>> projection)
            => throw new NotSupportedException($"Direct invocation of this method is not supported. It can only be invoked in the context of an {nameof(AsExpandable)} query.");
    }
}
