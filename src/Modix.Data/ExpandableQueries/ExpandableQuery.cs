using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Modix.Data.ExpandableQueries
{
    public class ExpandableQuery<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        public ExpandableQuery(ExpandableQueryProvider provider, Expression expression)
        {
            ElementType = typeof(T);
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public IEnumerator<T> GetEnumerator()
            => _provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
#pragma warning disable EF1001 // Internal EF Core API usage.
            => ((IAsyncQueryProvider)_provider).ExecuteAsync<IAsyncEnumerable<T>>(Expression).GetAsyncEnumerator(cancellationToken);
#pragma warning restore EF1001 // Internal EF Core API usage.

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public Type ElementType { get; }

        public IQueryProvider Provider
            => _provider;
        private readonly ExpandableQueryProvider _provider;

        public Expression Expression { get; }
    }
}
