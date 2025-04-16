using Modix.Models.Promotions;

namespace Modix.Web.Shared.Models.Promotions;

public record CampaignCommentData(long Id, PromotionSentiment PromotionSentiment, string Content, DateTimeOffset CreatedAt, bool IsFromCurrentUser);
