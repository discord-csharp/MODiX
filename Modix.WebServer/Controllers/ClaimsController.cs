using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [Route("~/api/config/claims")]
    public class ClaimsController : ModixController
    {
        private IClaimMappingRepository ClaimMappingRepository { get; }

        public ClaimsController(DiscordSocketClient client, IAuthorizationService modixAuth, IClaimMappingRepository claimMappingRepository) : base(client, modixAuth)
        {
            ClaimMappingRepository = claimMappingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> RoleClaims()
        {
            var found = await ClaimMappingRepository.SearchBriefsAsync(new ClaimMappingSearchCriteria
            {
                IsDeleted = false
            });

            return Ok(found);
        }

        [HttpPatch]
        public async Task<IActionResult> ModifyRole([FromBody]RoleClaimModifyData modifyData)
        {
            await ModixAuth.ModifyClaimMapping(modifyData.RoleId, modifyData.Claim, modifyData.MappingType);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            await ClaimMappingRepository.TryDeleteAsync(id, SocketUser.Id);
            return Ok();
        }
    }
}
