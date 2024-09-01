using Modix.Models.Promotions;

namespace Modix.Web.Shared.Models.Promotions;

public record PromotionCampaignData
{
    public required long Id { get; init; }
    public required ulong SubjectId { get; init; }
    public required string SubjectName { get; init; }
    public required ulong TargetRoleId { get; init; }
    public required string TargetRoleName { get; init; }
    public required PromotionCampaignOutcome? Outcome { get; set; }
    public required DateTimeOffset Created { get; init; }
    public required bool IsCurrentUserCampaign { get; init; }
    public required int ApproveCount { get; init; }
    public required int OpposeCount { get; init; }
    public required bool IsClosed { get; init; }
}
