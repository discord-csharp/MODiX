using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace Modix.Data.Projectables
{
    public class ProjectableQuery<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        public ProjectableQuery(ProjectableQueryProvider provider, Expression expression)
        {
            ElementType = typeof(T);
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public IEnumerator<T> GetEnumerator()
            => _provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
            => _provider.ExecuteAsync<T>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public Type ElementType { get; }

        public IQueryProvider Provider
            => _provider;
        private readonly ProjectableQueryProvider _provider;

        public Expression Expression { get; }
    }
}
