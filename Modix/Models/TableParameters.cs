namespace Modix.Models
{
    public class TableParameters
    {
        public int Page { get; set; }

        public int PerPage { get; set; }

        public SortParameter Sort { get; set; }

        public FilterParameter[] Filters { get; set; }
    }
}
