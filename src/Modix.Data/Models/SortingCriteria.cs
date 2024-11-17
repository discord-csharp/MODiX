namespace Modix.Data.Models;

public enum SortDirection
{
    Ascending,
    Descending
}

public class SortingCriteria
{
    public string PropertyName { get; set; } = null!;
    public SortDirection Direction { get; set; }
}
