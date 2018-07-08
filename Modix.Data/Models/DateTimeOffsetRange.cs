using System;

namespace Modix.Data.Models
{
    /// <summary>
    /// Describes an open-ended or close-ended range of <see cref="DateTimeOffset"/> values.
    /// </summary>
    public struct DateTimeOffsetRange
    {
        /// <summary>
        /// The earliest value to be included in the range.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// The latest value to be included in the range.
        /// </summary>
        public DateTimeOffset? To { get; set; }
    }
}
