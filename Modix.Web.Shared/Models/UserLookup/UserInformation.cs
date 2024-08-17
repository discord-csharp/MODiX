using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Shared.Models.UserLookup;

public record UserInformation(
    string Id,
    string? Username,
    string? Nickname,
    string? Discriminator,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? JoinedAt,
    DateTimeOffset? FirstSeen,
    DateTimeOffset? LastSeen,
    int Rank,
    int Last7DaysMessages,
    int Last30DaysMessages,
    decimal AverageMessagesPerDay,
    int Percentile,
    IEnumerable<RoleInformation> Roles,
    bool IsBanned,
    string? BanReason,
    bool IsGuildMember
);
