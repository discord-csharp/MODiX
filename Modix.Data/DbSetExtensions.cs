using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static IQueryable<bool> Select<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj, System.Linq.Expressions.Expression<Func<TEntity, bool>> selector) where TEntity : class
        {
            return System.Linq.Queryable.Select(obj, selector);
        }

        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(obj, predicate);
        }

        public static Task<TEntity> FirstOrDefaultAsync<TEntity>(this Microsoft.EntityFrameworkCore.DbSet<TEntity> obj) where TEntity : class
        {
            return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(obj);
        }
    }
}
