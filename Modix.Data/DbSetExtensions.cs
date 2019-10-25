using System;
using System.Collections.Generic;
using System.Linq;

namespace Modix.Data
{
    // Unfortunately this is required as per https://github.com/aspnet/EntityFrameworkCore/issues/18124
    // to fix ambiguous calls between EF and AsyncEnumerable in System.Interactive

    public static class DbSetExtensions
    {
        public static IAsyncEnumerable<TEntity> AsAsyncEnumerable<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj) where TEntity : class
        {
            return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsAsyncEnumerable(obj);
        }

        public static IQueryable<TEntity> Where<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return System.Linq.Queryable.Where(obj, predicate);
        }
    }
}
