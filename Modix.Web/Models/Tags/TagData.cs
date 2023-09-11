using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;

namespace Modix.Web.Models.Tags;

public record TagData(
        string Name,
        DateTimeOffset Created,
        bool IsOwnedByRole,
        GuildUserBrief? OwnerUser,
        GuildRoleBrief? OwnerRole,
        string? OwnerName,
        string Content,
        uint Uses,
        bool CanMaintain,
        TagSummary TagSummary)
{
    public static TagData CreateFromSummary(TagSummary summary)
    {
        return new TagData(
                summary.Name,
                summary.CreateAction.Created,
                summary.OwnerRole is not null,
                summary.OwnerUser,
                summary.OwnerRole,
                summary.OwnerRole?.Name ?? summary.OwnerUser?.Username,
                summary.Content,
                summary.Uses,
                false,
                summary);
    }
}
