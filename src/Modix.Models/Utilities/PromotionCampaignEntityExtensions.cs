namespace Modix.Models.Utilities;

public static class PromotionCampaignEntityExtensions
{
    public static readonly TimeSpan CampaignAcceptCooldown = TimeSpan.FromHours(48);

    public static TimeSpan GetTimeUntilCampaignCanBeClosed(this DateTimeOffset campaignCreationDate)
        => campaignCreationDate.Add(CampaignAcceptCooldown) - DateTimeOffset.UtcNow;

    public static DateTimeOffset GetExpectedCampaignCloseTimeStamp(this DateTimeOffset campaignCreationDate)
        => campaignCreationDate.Add(CampaignAcceptCooldown);
}
