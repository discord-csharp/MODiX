using Modix.Web.Models.Common;

namespace Modix.Web.Models.UserLookup;

public record RoleInformation(ulong Id, string Name, string Color) : IAutoCompleteItem;

