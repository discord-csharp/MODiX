using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Modix.Data.Repositories
{
    public delegate IQueryable<T> QueryClause<T>(IQueryable<T> query);

    public delegate IOrderedQueryable<T> OrderByClause<T>(IQueryable<T> query);

    public class QueryPageBuilder<T>
    {
        public IQueryable<T> Query { get; set; }

        public QueryClause<T> WhereClause { get; set; }

        public OrderByClause<T> OrderByClause { get; set; }

        public int? MaxPageSize { get; set; }

        public async Task<QueryPage<T>> BuildAsync(PagingCriteria pagingCriteria)
        {
            if (Query == null)
                return new QueryPage<T>()
                {
                    TotalRecordCount = 0,
                    FilteredRecordCount = 0,
                    Records = new T[0]
                };

            var filteredQuery = (WhereClause == null) ? Query : WhereClause(Query);

            var pagedQuery = (OrderByClause == null) ? filteredQuery : OrderByClause(filteredQuery);
            if (pagingCriteria.FirstRecordIndex.HasValue && (pagingCriteria.FirstRecordIndex.Value != 0))
                pagedQuery = pagedQuery.Skip(pagingCriteria.FirstRecordIndex.Value);
            if (pagingCriteria.LastRecordIndex.HasValue || MaxPageSize.HasValue)
                pagedQuery = pagedQuery.Take(Math.Min(pagingCriteria.PageSize.Value, MaxPageSize.Value));

            return new QueryPage<T>()
            {
                TotalRecordCount = await Query.LongCountAsync(),
                FilteredRecordCount = await filteredQuery.LongCountAsync(),
                Records = await pagedQuery.ToArrayAsync()
            };
        }
    }
}
