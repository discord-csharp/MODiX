#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Utilities;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [Route("~/api/autocomplete")]
    public class AutocompleteController : ModixController
    {
        private IDesignatedRoleService RoleService { get; }
        private readonly IUserService _userService;

        public AutocompleteController(DiscordSocketClient client, IAuthorizationService modixAuth, IDesignatedRoleService roleService, IUserService userService) : base(client, modixAuth)
        {
            RoleService = roleService;
            _userService = userService;
        }

        [HttpGet("channels")]
        public IActionResult AutocompleteChannels(string query)
        {
            if (query.StartsWith('#'))
            {
                query = query.Substring(1);
            }

            var result = UserGuild!.Channels
                .Where(d => d is SocketTextChannel)
                .Where(d => d.Name.OrdinalContains(query))
                .Take(10)
                .Select(d => new { d.Id, d.Name });

            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> AutocompleteUsersAsync(string query)
        {
            var result = UserGuild?.Users is null
                ? Enumerable.Empty<ModixUser>()
                : UserGuild.Users
                    .Where(d => d.Username.OrdinalContains(query) || d.Id.ToString() == query)
                    .Take(10)
                    .Select(ModixUser.FromIGuildUser);

            if (!result.Any() && ulong.TryParse(query, out var userId))
            {
                var user = await _userService.GetUserInformationAsync(UserGuild!.Id, userId);

                if(user != null)
                    result = result.Append(ModixUser.FromIGuildUser(user));
            }
            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> AutocompleteRolesAsync(string query, [FromQuery] bool rankOnly)
        {
            if (query.StartsWith('@'))
            {
                query = query.Substring(1);
            }

            if (rankOnly)
            {
                var criteria = new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = UserGuild!.Id,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false
                };

                IEnumerable<DesignatedRoleMappingBrief> result = await RoleService.SearchDesignatedRolesAsync(criteria);

                if (!string.IsNullOrWhiteSpace(query))
                {
                    result = result.Where(d => d.Role.Name.OrdinalContains(query));
                }

                return Ok(result.Take(10).Select(d => new { d.Role.Id, d.Role.Name }));
            }
            else
            {
                IEnumerable<IRole> result = UserGuild!.Roles;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    result = result.Where(d => d.Name.OrdinalContains(query));
                }

                return Ok(result.Take(10).Select(d => new { d.Id, d.Name }));
            }
        }
    }
}
