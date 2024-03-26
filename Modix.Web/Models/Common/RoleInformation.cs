namespace Modix.Web.Models.Common;

public record RoleInformation(ulong Id, string Name, string Color) : IAutoCompleteItem;
