namespace Modix.Web.Shared.Models.Common;

public sealed class ModixUser : IAutoCompleteItem
{
    public string? Name { get; init; }
    public ulong UserId { get; init; }
    public string? AvatarUrl { get; init; }
}
