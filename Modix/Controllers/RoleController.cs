using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [Route("~/api/config/roles")]
    public class RoleController : ModixController
    {
        private IDesignatedRoleService RoleService { get; }

        public RoleController(DiscordSocketClient client, IAuthorizationService modixAuth, IDesignatedRoleService roleService) : base(client, modixAuth)
        {
            RoleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> RoleDesignations()
        {
            var designatedRoles = await RoleService.GetDesignatedRolesAsync(ModixAuth.CurrentGuildId.Value);

            var mapped = designatedRoles.Select(d => new DesignatedRoleApiData
            {
                Id = d.Id,
                RoleId = d.Role.Id,
                RoleDesignation = d.Type,
                Name = UserGuild?.GetRole(d.Role.Id).Name ?? d.Id.ToString()
            });

            return Ok(mapped);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDesignation(long id)
        {
            await RoleService.RemoveDesignatedRoleByIdAsync(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> CreateDesignation([FromBody] DesignatedRoleCreationData creationData)
        {
            var foundRole = DiscordSocketClient
                ?.GetGuild(ModixAuth.CurrentGuildId.Value)
                ?.GetRole(creationData.RoleId);

            if (foundRole == null)
            {
                return BadRequest($"A role was not found with id {creationData.RoleId} in guild with id {ModixAuth.CurrentGuildId}");
            }

            foreach (var designation in creationData.RoleDesignations)
            {
                await RoleService.AddDesignatedRoleAsync(foundRole.Guild.Id, foundRole.Id, designation);
            }

            return Ok();
        }
    }
}
