using System;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace Modix.Data.Utilities
{
    /// <summary>
    /// Contains extension methods for operating on <see cref="DbContext"/> objects.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Marks a property of a particular entity as modified, within a <see cref="DbContext"/>,
        /// allowing that modification to be saved upon the next call to <see cref="DbContext.SaveChanges"/> (or equivalent).
        /// </summary>
        /// <typeparam name="TEntity">The type of <paramref name="entity"/>.</typeparam>
        /// <typeparam name="TValue">The type of property on <paramref name="entity"/> that has been modified.</typeparam>
        /// <param name="dbContext">The <see cref="DbContext"/> instance that is currently tracking <paramref name="entity"/>.</param>
        /// <param name="entity">The entity object whose property has been modified.</param>
        /// <param name="propertySelector">An expression defining the property that was modified.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="dbContext"/> and <paramref name="propertySelector"/>.</exception>
        public static void UpdateProperty<TEntity, TValue>(this DbContext dbContext, TEntity entity, Expression<Func<TEntity, TValue>> propertySelector) where TEntity : class
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            dbContext.Entry(entity).Property(propertySelector).IsModified = true;
        }
    }
}
