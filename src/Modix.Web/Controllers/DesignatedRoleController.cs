using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Models.Core;
using Modix.Services.Core;
using Modix.Web.Shared.Models.Configuration;

namespace Modix.Web.Controllers;

[Route("~/api/config/roles")]
[ApiController]
[Authorize]
public class DesignatedRoleController : ModixController
{
    private readonly IDesignatedRoleService _designatedRoleService;

    public DesignatedRoleController(IDesignatedRoleService designatedRoleService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _designatedRoleService = designatedRoleService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedRoleMappingRead))]
    public async Task<Dictionary<DesignatedRoleType, List<DesignatedRoleData>>> GetRoleDesignationsAsync()
    {
        var designatedRoles = await _designatedRoleService.GetDesignatedRolesAsync(UserGuild.Id);

        return designatedRoles
            .Select(d => new DesignatedRoleData(
                d.Id,
                d.Role.Id,
                d.Type,
                UserGuild.GetRole(d.Role.Id)?.Name ?? d.Role.Name))
            .ToLookup(x => x.RoleDesignation, x => x)
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    [HttpPut("{roleId}/{designatedRoleType}")]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedRoleMappingCreate))]
    public async Task<IActionResult> CreateDesignationAsync(ulong roleId, DesignatedRoleType designatedRoleType)
    {
        var foundRole = UserGuild.GetRole(roleId);

        if (foundRole is null)
            return BadRequest($"A role was not found with id {roleId} in guild with id {ModixAuth.CurrentGuildId}");

        var id = await _designatedRoleService.AddDesignatedRoleAsync(UserGuild.Id, roleId, (DesignatedRoleType)(int)designatedRoleType);

        return Ok(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedRoleMappingDelete))]
    public async Task RemoveDesignationAsync(long id)
    {
        await _designatedRoleService.RemoveDesignatedRoleByIdAsync(id);
    }
}
