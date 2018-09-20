using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Data.Projectables
{
    public class ProjectableQueryProvider : IAsyncQueryProvider
    {
        public ProjectableQueryProvider(IAsyncQueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
            => new ProjectableQuery<T>(this, expression);

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var elementType = expression.Type.GetElementType();

            try
            {
                return Activator.CreateInstance(typeof(ProjectableQuery<>).MakeGenericType(elementType), new object[] { this, expression }) as IQueryable;
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
            => _provider.ExecuteAsync<TResult>(Visit(expression));

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => _provider.ExecuteAsync<TResult>(Visit(expression), cancellationToken);

        internal readonly IAsyncQueryProvider _provider;

        private Expression Visit(Expression expression)
            => new ProjectableVisitor(_provider).Visit(expression);
    }
}
