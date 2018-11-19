using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
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
            await ModixAuth.ModifyClaimMappingAsync(modifyData.RoleId, modifyData.Claim, modifyData.MappingType);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            using (var transaction = await ClaimMappingRepository.BeginDeleteTransactionAsync())
            {
                if (!await ClaimMappingRepository.TryDeleteAsync(id, SocketUser.Id))
                    return NotFound();

                transaction.Commit();
            }
            return Ok();
        }
    }
}
