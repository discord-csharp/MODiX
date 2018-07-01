using System;

namespace Modix.Data.Repositories
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
        public ulong? FirstRecordIndex { get; set; }

        /// <summary>
        /// The 0-based index of the last record to be returned.
        /// A value of null indicates that all records following <see cref="FirstRecordIndex"/> should be returned.
        /// This value is coerced, based on <see cref="MaxPageSize"/>. That is, the value returned by this property
        /// may not always be the last value that was assigned to this property.
        /// </summary>
        public ulong? LastRecordIndex
        {
            get => MaxPageSize.HasValue
                ? Math.Min(_lastRecordIndex.Value, (MaxPageSize.Value - 1) + (FirstRecordIndex ?? 0))
                : _lastRecordIndex;
            set => _lastRecordIndex = value;
        }
        private ulong? _lastRecordIndex;

        /// <summary>
        /// The total number of records to be returned, as defined by <see cref="FirstRecordIndex"/>, <see cref="LastRecordIndex"/> and <see cref="MaxPageSize"/>.
        /// A value of null indicates that all records are to be returned.
        /// </summary>
        public ulong? PageSize
        {
            get => MaxPageSize.HasValue
                ? Math.Min(MaxPageSize.Value, RealPageSize)
                : RealPageSize;
            set => LastRecordIndex = (value - 1) + (FirstRecordIndex ?? 0);
        }

        /// <summary>
        /// The maximum number of records to be returned.
        /// A value of null indicates that <see cref="PageSize"/> should be determined only by <see cref="FirstRecordIndex"/> and <see cref="LastRecordIndex"/>.
        /// </summary>
        public ulong? MaxPageSize { get; set; }

        private ulong RealPageSize
            => (_lastRecordIndex ?? ulong.MaxValue) - (FirstRecordIndex ?? 0) + 1;
    }
}
