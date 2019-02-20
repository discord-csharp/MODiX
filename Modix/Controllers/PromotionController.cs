using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
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

            if (result == null) { return NotFound(); }

            //TODO: Map this properly
            return Ok(result.Comments.Select(c => new
            {
                c.Id,
                c.Sentiment,
                c.Content,
                CreateAction = new { c.CreateAction.Id, c.CreateAction.Created },
                IsModified = !(c.ModifyAction is null),
                IsFromCurrentUser = c.CreateAction.CreatedBy.Id == ModixAuth.CurrentUserId,
            }));
        }

        [HttpGet("{subjectId}/nextRank")]
        public async Task<IActionResult> GetNextRankRoleForUser(ulong subjectId)
        {
            var result = await _promotionsService.GetNextRankRoleForUserAsync(subjectId);
            var color = result is null
                ? Color.DarkGrey
                : UserGuild.Roles.FirstOrDefault(r => r.Id == result.Id).Color;

            return Ok(new { result?.Id, result?.Name, fgColor = color.ToString() });
        }

        [HttpPut("{campaignId}/comments")]
        public async Task<IActionResult> AddComment(int campaignId, [FromBody] PromotionCommentData commentData)
        {
            var campaigns = await _promotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria
            {
                Id = campaignId
            });

            if (!campaigns.Any())
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

        [HttpPut("{commentId}/updateComment")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] PromotionCommentData commentData)
        {
            try
            {
                await _promotionsService.UpdateCommentAsync(commentId, commentData.Sentiment, commentData.Body);
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
                await _promotionsService.AcceptCampaignAsync(campaignId, false);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("{campaignId}/forceAccept")]
        public async Task<IActionResult> ForceAcceptCampaign(int campaignId)
        {
            try
            {
                await _promotionsService.AcceptCampaignAsync(campaignId, true);
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
                await _promotionsService.CreateCampaignAsync(creationData.UserId, creationData.Comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
