namespace Modix.Web.Shared.Models.Tags;

public record TagData(
        string Name,
        DateTimeOffset Created,
        bool IsOwnedByRole,
        ulong OwnerId,
        string OwnerName,
        string Content,
        uint Uses,
        bool CanMaintain);
