using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    /// <summary>
    /// Describes a set of criteria for selecting a continguous subset of records within a larger recordset.
    /// </summary>
    public class PagingCriteria
    {
        /// <summary>
        /// The 0-based index of the first record to be returned.
        /// A value of null is equivalent to 0.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? FirstRecordIndex { get; set; }

        /// <summary>
        /// The 0-based index of the last record to be returned.
        /// A value of null indicates that all records following <see cref="FirstRecordIndex"/> should be returned.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? LastRecordIndex { get; set; }
        
        /// <summary>
        /// The total number of records to be returned.
        /// A value of null indicates that all records following <see cref="FirstRecordIndex"/> should be returned.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? PageSize
        {
            get => LastRecordIndex.HasValue
                ? (LastRecordIndex.Value + 1) - (FirstRecordIndex ?? 0)
                : null as int?;
            set => LastRecordIndex = value.HasValue
                ? (value.Value - 1) + (FirstRecordIndex ?? 0)
                : null as int?;
        }
    }
}
