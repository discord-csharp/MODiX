﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query.Internal;

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

            var elementType = expression.Type.GetElementType()!;

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(ExpandableQuery<>).MakeGenericType(elementType), new object[] { this, expression })!;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is object)
                {
                    throw ex.InnerException;
                }
                throw;
            }
        }

        public TResult Execute<TResult>(Expression expression)
            => _provider.Execute<TResult>(Visit(expression));

        public object? Execute(Expression expression)
            => _provider.Execute(Visit(expression));

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
#pragma warning disable EF1001 // Internal EF Core API usage.
            => ((IAsyncQueryProvider)_provider).ExecuteAsync<TResult>(Visit(expression));
#pragma warning restore EF1001 // Internal EF Core API usage.

        internal readonly IQueryProvider _provider;

        private Expression Visit(Expression expression)
            => new ProjectMethodVisitor().Visit(
                new ExpansionExpressionVisitor().Visit(expression));
    }
}
