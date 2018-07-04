using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.Promotions;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [Route("~/api")]
    public class PromotionController : ModixController
    {
        private readonly PromotionService _promotionService;

        public PromotionController(DiscordSocketClient client, PromotionService promotionService) : base(client)
        {
            _promotionService = promotionService;
        }

        [HttpGet("campaigns")]
        public async Task<IActionResult> Campaigns()
        {
            return Ok(await _promotionService.GetCampaigns());
        }

        [HttpPut("campaigns/{campaignId}/comments")]
        public async Task<IActionResult> AddComment(int campaignId, [FromBody] PromotionCommentData commentData)
        {
            var campaign = await _promotionService.GetCampaign(campaignId);

            if (campaign == null) return BadRequest($"Invalid campaign ID specified ({campaignId})");

            try
            {
                await _promotionService.AddComment(campaign, commentData.Body, commentData.Sentiment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("campaigns/{campaignId}/approve")]
        public async Task<IActionResult> ApproveCampaign(int campaignId)
        {
            try
            {
                await _promotionService.ApproveCampaign(SocketUser, await _promotionService.GetCampaign(campaignId));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("campaigns/{campaignId}/deny")]
        public async Task<IActionResult> DenyCampaign(int campaignId)
        {
            await _promotionService.DenyCampaign(SocketUser, await _promotionService.GetCampaign(campaignId));
            return Ok();
        }

        [HttpPost("campaigns/{campaignId}/activate")]
        public async Task<IActionResult> ActivateCampaign(int campaignId)
        {
            await _promotionService.ActivateCampaign(SocketUser, await _promotionService.GetCampaign(campaignId));
            return Ok();
        }

        [HttpPut("campaigns")]
        public async Task<IActionResult> Create([FromBody] PromotionCreationData creationData)
        {
            var foundUser = _client.Guilds.First().GetUser(creationData?.UserId ?? 0);

            if (foundUser == null) return BadRequest($"User not found.");

            try
            {
                await _promotionService.CreateCampaign(foundUser, creationData.Comment);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}