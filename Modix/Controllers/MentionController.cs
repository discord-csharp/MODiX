using System;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;

using Modix.Data.Models.Mentions;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.Mentions;

namespace Modix.Controllers
{
    [Route("~/api/config/mentions")]
    public class MentionController : ModixController
    {
        private IMentionService MentionService { get; }

        public MentionController(DiscordSocketClient client, IAuthorizationService modixAuth, IMentionService mentionService) : base(client, modixAuth)
        {
            MentionService = mentionService;
        }

        [HttpGet("mentionabilityTypes")]
        public IActionResult GetMentionabilityTypes()
            => Ok(MentionabilityData.GetMentionabilityTypes());

        [HttpGet("{roleId}/mentionData")]
        public async Task<IActionResult> GetMentionDataAsync(ulong roleId)
        {
            var mentionMapping = await MentionService.GetMentionMappingAsync(roleId);

            return Ok(new
            {
                mentionMapping.Role,
                Mentionability = mentionMapping.Mentionability.ToString(),
                mentionMapping.MinimumRank,
            });
        }

        [HttpPatch("{roleId}/mentionability/{mentionability}")]
        public async Task<IActionResult> UpdateMentionabilityAsync(ulong roleId, string mentionability)
        {
            if (!Enum.TryParse<MentionabilityType>(mentionability, out var mentionabilityType))
                return BadRequest($"{mentionability} is not a valid mentionability type.");

            return await MentionService.TryUpdateMentionMappingAsync(roleId, x => x.Mentionability = mentionabilityType)
                ? (IActionResult)Ok()
                : (IActionResult)BadRequest($"Unable to update the mention mapping for role {roleId}.");
        }

        [HttpPatch("{roleId}/minimumRank/{minimumRankId}")]
        public async Task<IActionResult> UpdateMinimumRankAsync(ulong roleId, ulong? minimumRankId)
            => await MentionService.TryUpdateMentionMappingAsync(roleId, x => x.MinimumRankId = minimumRankId)
                ? (IActionResult)Ok()
                : (IActionResult)BadRequest($"Unable to update the mention mapping for role {roleId}.");
    }
}
