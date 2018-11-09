using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Promotions;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.Promotions;

namespace Modix.Controllers
{
    [Route("~/api/campaigns")]
    public class PromotionController : ModixController
    {
        private readonly IPromotionsService _promotionsService;

        public PromotionController(DiscordSocketClient client, IPromotionsService promotionService, IAuthorizationService auth) : base(client, auth)
        {
            _promotionsService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> Campaigns()
            => Ok(await _promotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria
            {
                GuildId = UserGuild.Id
            }));

        [HttpGet("{campaignId}")]
        public async Task<IActionResult> CampaignComments(long campaignId)
        {
            var result = await _promotionsService.GetCampaignDetailsAsync(campaignId);
            return Ok(result.Comments);
        }

        [HttpPut("{campaignId}/comments")]
        public async Task<IActionResult> AddComment(int campaignId, [FromBody] PromotionCommentData commentData)
        {
            var campaign = await _promotionsService.GetCampaignDetailsAsync(campaignId);

            if (campaign == null)
            {
                return BadRequest($"Invalid campaign ID specified ({campaignId})");
            }

            try
            {
                await _promotionsService.AddCommentAsync(campaignId, commentData.Sentiment, commentData.Body);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("{campaignId}/accept")]
        public async Task<IActionResult> AcceptCampaign(int campaignId)
        {
            try
            {
                await _promotionsService.AcceptCampaignAsync(campaignId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("{campaignId}/reject")]
        public async Task<IActionResult> RejectCampaign(int campaignId)
        {
            try
            {
                await _promotionsService.RejectCampaignAsync(campaignId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Create([FromBody] PromotionCreationData creationData)
        {
            try
            {
                // TODO: get promotion rank from creation data
                await _promotionsService.CreateCampaignAsync(creationData.UserId, creationData.RoleId, creationData.Comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
