namespace Modix.Data.Models
{
    /// <summary>
    /// Defines the possible directions by which a set of records may be sorted.
    /// </summary>
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// Describes a set of criteria for sorting a set of records, according to a single sortable property.
    /// </summary>
    public class SortingCriteria
    {
        /// <summary>
        /// The name of the property to be sorted.
        /// </summary>
        public string PropertyName { get; set; } = null!;

        /// <summary>
        /// The direction of the sort to be applied.
        /// </summary>
        public SortDirection Direction { get; set; }
    }
}
