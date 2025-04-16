using Modix.Models.Core;

namespace Modix.Models.Utilities;

[AttributeUsage(AttributeTargets.Field)]
public class ClaimInfoAttribute : Attribute
{
    public AuthorizationClaimCategory Category { get; set; }
    public string Description { get; set; }

    public ClaimInfoAttribute(AuthorizationClaimCategory category, string description)
    {
        Category = category;
        Description = description;
    }
}
