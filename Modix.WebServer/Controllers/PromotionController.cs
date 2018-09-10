using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Promotions;
using Modix.Services.Core;
using Modix.Services.Promotions;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [Route("~/api")]
    public class PromotionController : ModixController
    {
        private IPromotionsService _promotionsService;

        public PromotionController(DiscordSocketClient client, IPromotionsService promotionService, IAuthorizationService auth) : base(client, auth)
        {
            _promotionsService = promotionService;
        }

        [HttpGet("campaigns")]
        public async Task<IActionResult> Campaigns()
            => Ok(await _promotionsService.SearchCampaignsAsync(null));
        
        [HttpPut("campaigns/{campaignId}/comments")]
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
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("campaigns/{campaignId}/accept")]
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

        [HttpPost("campaigns/{campaignId}/reject")]
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

        [HttpPut("campaigns")]
        public async Task<IActionResult> Create([FromBody] PromotionCreationData creationData)
        {
            throw new NotImplementedException();

            try
            {
                // TODO: get promotion rank from creation data
                await _promotionsService.CreateCampaignAsync(creationData.UserId, 0, creationData.Comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
