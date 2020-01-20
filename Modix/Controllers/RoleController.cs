using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [ApiController]
    [Route("~/api/config/roles")]
    public class RoleController : ModixController
    {
        private readonly IDesignatedRoleService _roleService;

        public RoleController(DiscordSocketClient client, IAuthorizationService modixAuth, IDesignatedRoleService roleService) : base(client, modixAuth)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> RoleDesignationsAsync()
        {
            var designatedRoles = await _roleService.GetDesignatedRolesAsync(ModixAuth.CurrentGuildId.Value);

            var mapped = designatedRoles.Select(d => new DesignatedRoleApiData
            {
                Id = d.Id,
                RoleId = d.Role.Id,
                RoleDesignation = d.Type,
                Name = UserGuild?.GetRole(d.Role.Id)?.Name ?? d.Role.Id.ToString()
            });

            return Ok(mapped);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDesignationAsync(long id)
        {
            await _roleService.RemoveDesignatedRoleByIdAsync(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> CreateDesignationAsync(DesignatedRoleCreationData creationData)
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
                await _roleService.AddDesignatedRoleAsync(foundRole.Guild.Id, foundRole.Id, designation);
            }

            return Ok();
        }
    }
}
