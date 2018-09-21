using System;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Modix.Data.Projectables
{
    public static class ProjectableExtensions
    {
        public static IQueryable<T> AsProjectable<T>(this IQueryable<T> query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (query is ProjectableQuery<T>)
                return query;

            var provider = query.Provider as IAsyncQueryProvider;
            if (provider is null)
                throw new ArgumentException($"{nameof(query.Provider)} must be an {nameof(IAsyncQueryProvider)}", nameof(query));

            return new ProjectableQueryProvider(provider).CreateQuery<T>(query.Expression);
        }

        public static TOut Project<TIn, TOut>(this TIn input, Expression<Func<TIn, TOut>> projection)
            => throw new NotSupportedException($"Direct invocation of this method is not supported. It can only be invoked in the context of an {nameof(AsProjectable)} query.");
    }
}
