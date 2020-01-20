using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [ApiController]
    [Route("~/api/config/claims")]
    public class ClaimsController : ModixController
    {
        private readonly IClaimMappingRepository _claimMappingRepository;

        public ClaimsController(DiscordSocketClient client, IAuthorizationService modixAuth, IClaimMappingRepository claimMappingRepository) : base(client, modixAuth)
        {
            _claimMappingRepository = claimMappingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> RoleClaimsAsync()
        {
            var found = await _claimMappingRepository.SearchBriefsAsync(new ClaimMappingSearchCriteria
            {
                IsDeleted = false
            });

            return Ok(found);
        }

        [HttpPatch]
        public async Task<IActionResult> ModifyRoleAsync(RoleClaimModifyData modifyData)
        {
            await ModixAuth.ModifyClaimMappingAsync(modifyData.RoleId, modifyData.Claim, modifyData.MappingType);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaimAsync(int id)
        {
            using (var transaction = await _claimMappingRepository.BeginDeleteTransactionAsync())
            {
                if (!await _claimMappingRepository.TryDeleteAsync(id, SocketUser.Id))
                    return NotFound();

                transaction.Commit();
            }
            return Ok();
        }
    }
}
