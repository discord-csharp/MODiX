using Modix.Data.Models.Promotions;

namespace Modix.Web.Models.Promotions;

public record CampaignCommentData(long Id, PromotionSentiment PromotionSentiment, string Content, DateTimeOffset CreatedAt, bool IsFromCurrentUser);
