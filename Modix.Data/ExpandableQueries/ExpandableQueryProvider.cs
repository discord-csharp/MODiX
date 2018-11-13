using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Data.ExpandableQueries
{
    public class ExpandableQueryProvider : IAsyncQueryProvider
    {
        public ExpandableQueryProvider(IQueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
            => new ExpandableQuery<T>(this, expression);

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var elementType = expression.Type.GetElementType();

            try
            {
                return Activator.CreateInstance(typeof(ExpandableQuery<>).MakeGenericType(elementType), new object[] { this, expression }) as IQueryable;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
            => _provider.Execute<TResult>(Visit(expression));

        public object Execute(Expression expression)
            => _provider.Execute(Visit(expression));

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => (_provider is IAsyncQueryProvider asyncProvider)
                ? asyncProvider.ExecuteAsync<TResult>(Visit(expression))
                : throw new InvalidOperationException("This query cannot be executed asynchronously");

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => (_provider is IAsyncQueryProvider asyncProvider)
                ? asyncProvider.ExecuteAsync<TResult>(Visit(expression), cancellationToken)
                : throw new InvalidOperationException("This query cannot be executed asynchronously");

        internal readonly IQueryProvider _provider;

        private Expression Visit(Expression expression)
            => new ProjectMethodVisitor().Visit(
                new ExpansionExpressionVisitor().Visit(expression));
    }
}
