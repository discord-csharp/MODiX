using System.Collections.Generic;

namespace Modix.Data.Models
{
    /// <summary>
    /// Describes paged subset of records, from a larger recordset.
    /// </summary>
    /// <typeparam name="T">The type of record contained in the page.</typeparam>
    public class RecordsPage<T>
    {
        /// <summary>
        /// The total number of records in the recordset.
        /// </summary>
        public long TotalRecordCount { get; set; }

        /// <summary>
        /// The number of records in the recordset, after applying an optional set of filtering criteria.
        /// </summary>
        public long FilteredRecordCount { get; set; }

        /// <summary>
        /// The current page of records, selected from the larger recordset.
        /// </summary>
        public IReadOnlyCollection<T> Records { get; set; } = null!;
    }
}
