using System;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;

namespace Modix.Data.ExpandableQueries
{
    public static class ExpandableExtensions
    {
        static ExpandableExtensions()
        {
            LinqKitExtension.QueryOptimizer = QueryOptimizer;
        }

        private static Expression QueryOptimizer(Expression expression)
            => new ProjectMethodVisitor().Visit(
                            new ExpansionExpressionVisitor().Visit(expression));

        public static IQueryable<T> AsExpandable<T>(this IQueryable<T> query)
        {
            return LinqKit.Extensions.AsExpandable(query);
        }

        public static TOut Project<TIn, TOut>(this TIn input, Expression<Func<TIn, TOut>> projection)
            where TIn : notnull
            => throw new NotSupportedException($"Direct invocation of this method is not supported. It can only be invoked in the context of an {nameof(AsExpandable)} query.");
    }
}
