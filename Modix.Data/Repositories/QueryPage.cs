using System.Collections.Generic;

namespace Modix.Data.Repositories
{
    public class QueryPage<T>
    {
        public long TotalRecordCount { get; set; }

        public long FilteredRecordCount { get; set; }

        public IReadOnlyCollection<T> Records { get; set; }
    }
}
