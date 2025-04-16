using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Models.Core;
using Modix.Services.Promotions;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.Promotions;

namespace Modix.Web.Controllers;

[Route("~/api/campaigns")]
[ApiController]
[Authorize(Roles = nameof(AuthorizationClaim.PromotionsRead))]
public class CampaignController : ModixController
{
    private readonly IPromotionsService _promotionsService;

    public CampaignController(IPromotionsService promotionsService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _promotionsService = promotionsService;
    }

    [HttpGet]
    public async IAsyncEnumerable<PromotionCampaignData> GetCampaignsAsync()
    {
        var campaigns = await _promotionsService.SearchCampaignsAsync(new Data.Models.Promotions.PromotionCampaignSearchCriteria
        {
            GuildId = UserGuild.Id
        });

        foreach (var campaign in campaigns)
        {
            yield return new PromotionCampaignData
            {
                Id = campaign.Id,
                SubjectId = campaign.Subject.Id,
                SubjectName = campaign.Subject.GetFullUsername(),
                TargetRoleId = campaign.TargetRole.Id,
                TargetRoleName = campaign.TargetRole.Name,
                Outcome = campaign.Outcome,
                Created = campaign.CreateAction.Created,
                IsCurrentUserCampaign = campaign.Subject.Id == SocketUser.Id,
                ApproveCount = campaign.ApproveCount,
                OpposeCount = campaign.OpposeCount,
                IsClosed = campaign.CloseAction is not null
            };
        }
    }

    [HttpGet("{campaignId}")]
    public async Task<IActionResult> GetCampaignCommentsAsync(long campaignId)
    {
        var campaignDetails = await _promotionsService.GetCampaignDetailsAsync(campaignId);

        if (campaignDetails is null)
            return NotFound();

        var campaignComments = campaignDetails.Comments
            .Where(x => x.ModifyAction is null)
            .Select(x => new CampaignCommentData
                (
                    x.Id,
                    x.Sentiment,
                    x.Content,
                    x.CreateAction.Created,
                    x.CreateAction.CreatedBy.Id == SocketUser.Id
                )
            )
            .ToDictionary(x => x.Id);

        return Ok(campaignComments);
    }

    [HttpPut("{campaignId}/createcomment")]
    public async Task<CampaignCommentData> CreateCommentAsync(long campaignId, [FromBody] CampaignCommentData campaignCommentData)
    {
        var promotionActionSummary = await _promotionsService.AddCommentAsync(
            campaignId,
            campaignCommentData.PromotionSentiment,
            campaignCommentData.Content);

        var newComment = promotionActionSummary.NewComment;

        return new CampaignCommentData(newComment.Id, newComment.Sentiment, newComment.Content, promotionActionSummary.Created, true);
    }

    [HttpPatch("updatecomment")]
    public async Task<CampaignCommentData> UpdateCommentAsync([FromBody] CampaignCommentData campaignCommentData)
    {
        var promotionActionSummary = await _promotionsService.UpdateCommentAsync(
            campaignCommentData.Id,
            campaignCommentData.PromotionSentiment,
            campaignCommentData.Content);

        var newComment = promotionActionSummary.NewComment;

        return new CampaignCommentData(newComment.Id, newComment.Sentiment, newComment.Content, promotionActionSummary.Created, true);
    }

    [HttpPost("{campaignId}/accept/{force}")]
    [Authorize(Roles = nameof(AuthorizationClaim.PromotionsCloseCampaign))]
    public async Task AcceptCampaignAsync(long campaignId, bool force)
    {
        await _promotionsService.AcceptCampaignAsync(campaignId, force);
    }

    [HttpPost("{campaignId}/reject")]
    [Authorize(Roles = nameof(AuthorizationClaim.PromotionsCloseCampaign))]
    public async Task RejectCampaignAsync(long campaignId)
    {
        await _promotionsService.RejectCampaignAsync(campaignId);
    }

    [HttpGet("{subjectId}/nextrank")]
    [Authorize(Roles = nameof(AuthorizationClaim.PromotionsCreateCampaign))]
    public async Task<NextRank> GetNextRankRoleForUserAsync(ulong subjectId)
    {
        var nextRank = await _promotionsService.GetNextRankRoleForUserAsync(subjectId);

        if (nextRank is null)
            return new NextRank("None", "#607d8b");

        var color = UserGuild.Roles.First(r => r.Id == nextRank.Id).Color;
        return new NextRank(nextRank.Name, color.ToString());
    }

    [HttpPut("create")]
    [Authorize(Roles = nameof(AuthorizationClaim.PromotionsCreateCampaign))]
    public async Task CreateAsync([FromBody] PromotionCreationData creationData)
    {
        await _promotionsService.CreateCampaignAsync(creationData.UserId, creationData.Comment);
    }
}
