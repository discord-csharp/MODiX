using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Modix.Data.Models;

namespace Modix.Data.Utilities
{
    /// <summary>
    /// Contains extension methods for operating on <see cref="IQueryable{T}"/> objects.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Attaches an <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
        /// or <see cref="Queryable.OrderByDescending{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
        /// clause to a query, based on a given <see cref="SortDirection"/> value.
        /// </summary>
        /// <typeparam name="TRecord">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <typeparam name="TKey">The type of the ordering key.</typeparam>
        /// <param name="source">The query to which the new clause should be attached.</param>
        /// <param name="keySelector">An expression defining the key values to be used for sorting.</param>
        /// <param name="direction">The direction of the sort to be applied.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/> and <paramref name="keySelector"/>.</exception>
        /// <returns>The result of attaching the new OrderBy clause to <paramref name="source"/>.</returns>
        public static IOrderedQueryable<TRecord> OrderBy<TRecord, TKey>(this IQueryable<TRecord> source, Expression<Func<TRecord, TKey>> keySelector, SortDirection direction)
            => (direction == SortDirection.Ascending)
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);

        /// <summary>
        /// Attaches an <see cref="Queryable.ThenBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
        /// or <see cref="Queryable.ThenByDescending{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
        /// clause to a query, based on a given <see cref="SortDirection"/> value.
        /// </summary>
        /// <typeparam name="TRecord">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <typeparam name="TKey">The type of the ordering key.</typeparam>
        /// <param name="source">The query to which the new clause should be attached.</param>
        /// <param name="keySelector">An expression defining the key values to be used for sorting.</param>
        /// <param name="direction">The direction of the sort to be applied.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/> and <paramref name="keySelector"/>.</exception>
        /// <returns>The result of attaching the new ThenBy clause to <paramref name="source"/>.</returns>
        public static IOrderedQueryable<TRecord> ThenBy<TRecord, TKey>(this IOrderedQueryable<TRecord> source, Expression<Func<TRecord, TKey>> keySelector, SortDirection direction)
            => (direction == SortDirection.Ascending)
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);

        /// <summary>
        /// Attaches an <see cref="OrderBy{TRecord, TKey}(IQueryable{TRecord}, Expression{Func{TRecord, TKey}}, SortDirection)"/>
        /// or <see cref="ThenBy{TRecord, TKey}(IOrderedQueryable{TRecord}, Expression{Func{TRecord, TKey}}, SortDirection)"/>
        /// clause to a query, based on a given sort key, and <see cref="SortDirection"/> value.
        /// This allows for chained ordering clauses to be built, dynamically, as this method determines whether to append OrderBy or ThenBy
        /// by inspecting the existing query to see if the previous clause was an OrderBy or ThenBy.
        /// </summary>
        /// <typeparam name="TRecord">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <typeparam name="TKey">The type of the ordering key.</typeparam>
        /// <param name="query">The query to which the new clause should be attached.</param>
        /// <param name="keySelector">An expression defining the key values to be used for sorting.</param>
        /// <param name="direction">The direction of the sort to be applied.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/> and <paramref name="keySelector"/>.</exception>
        /// <returns>The result of attaching the new OrderBy or ThenBy clause to <paramref name="query"/>.</returns>
        public static IOrderedQueryable<TRecord> OrderThenBy<TRecord, TKey>(this IQueryable<TRecord> query, Expression<Func<TRecord, TKey>> keySelector, SortDirection direction)
            => (query.Expression.Type == typeof(IOrderedQueryable<TRecord>))
                ? ((IOrderedQueryable<TRecord>)query).ThenBy(keySelector, direction)
                : query.OrderBy(keySelector, direction);


        /// <summary>
        /// Filters a query by attaching a <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/> clause
        /// to it, based on a given set of criteria.
        /// </summary>
        /// <typeparam name="T">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <param name="source">The query to which the new clause should be attached.</param>
        /// <param name="predicate">An expression defining the Where clause to be attached.</param>
        /// <param name="criteria">A flag indicating whether the described Where clause should be attached (true) or omitted (false).</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/> and <paramref name="predicate"/>.</exception>
        /// <returns>If <paramref name="criteria"/> is true, the result of attaching the new Where clause to <paramref name="source"/>. Otherwise, just <paramref name="source"/>.</returns>
        public static IQueryable<T> FilterBy<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool criteria)
            => criteria
                ? source.Where(predicate)
                : source;

        /// <summary>
        /// Attaches <see cref="OrderThenBy{TRecord, TKey}(IQueryable{TRecord}, Expression{Func{TRecord, TKey}}, SortDirection)"/> clauses to a query,
        /// based on a given set of criteria, and a map that defines the sortable properties of the records in the query, and how to select them.
        /// </summary>
        /// <typeparam name="T">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <param name="source">The query to which the new clause should be attached.</param>
        /// <param name="criteria">An optional set of <see cref="SortingCriteria"/>, defining the ordering clauses to be attached.</param>
        /// <param name="sortablePropertyMap">A map of valid <see cref="SortingCriteria.PropertyName"/> values, to expressions that select the appropriate sorting key for that sorting clause.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/> and <paramref name="sortablePropertyMap"/>.</exception>
        /// <returns>The result of attaching a sequence of OrderThenBy clauses to <paramref name="source"/>.</returns>
        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, IEnumerable<SortingCriteria>? criteria, IDictionary<string, Expression<Func<T, object?>>> sortablePropertyMap)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (sortablePropertyMap == null)
                throw new ArgumentNullException(nameof(sortablePropertyMap));

            if (criteria == null)
                return source;

            foreach(var item in criteria)
            {
                if (!sortablePropertyMap.TryGetValue(item.PropertyName, out var selector))
                    throw new InvalidOperationException($"{item.PropertyName} is not a sortable property of {typeof(T).FullName}");

                source = source.OrderThenBy(selector, item.Direction);
            }

            return source;
        }

        /// <summary>
        /// Attaches <see cref="Queryable.Skip{TSource}(IQueryable{TSource}, int)"/> and <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)"/>
        /// clauses to a query, based on a given set of criteria.
        /// </summary>
        /// <typeparam name="T">The <see cref="IQueryable{T}"/> record type.</typeparam>
        /// <param name="source">The query to which the new clause should be attached.</param>
        /// <param name="criteria">An optional set of <see cref="PagingCriteria"/>, defining the paging clauses to be attached.</param>
        /// <param name="maxPageSize">An optional maximum number of records to be selected.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="source"/>.</exception>
        /// <returns>The result of attaching the Skip and/or Take clauses to <paramref name="source"/>.</returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> source, PagingCriteria criteria, int? maxPageSize = null)
        {
            if (criteria != null)
            {
                if (criteria.FirstRecordIndex.HasValue && (criteria.FirstRecordIndex.Value != 0))
                    source = source.Skip(criteria.FirstRecordIndex.Value);

                if(criteria.LastRecordIndex.HasValue || maxPageSize.HasValue)
                    source = source.Take(Math.Min(criteria.PageSize ?? int.MaxValue, maxPageSize ?? int.MaxValue));
            }

            return source;
        }
    }
}
