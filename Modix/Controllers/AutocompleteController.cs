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

        public AutocompleteController(DiscordSocketClient client, IAuthorizationService modixAuth, IDesignatedRoleService roleService) : base(client, modixAuth)
        {
            RoleService = roleService;
        }

        [HttpGet("channels")]
        public IActionResult AutocompleteChannels(string query)
        {
            if (query.StartsWith('#'))
            {
                query = query.Substring(1);
            }

            var result = UserGuild.Channels
                .Where(d => d is SocketTextChannel)
                .Where(d => d.Name.OrdinalContains(query))
                .Take(10)
                .Select(d => new { d.Id, d.Name });

            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> AutocompleteUsers(string query)
        {
            await UserGuild.DownloadUsersAsync();

            var result = UserGuild.Users is null
                ? Enumerable.Empty<ModixUser>()
                : UserGuild.Users
                    .Where(d => d.Username.OrdinalContains(query))
                    .Take(10)
                    .Select(d => ModixUser.FromSocketGuildUser(d));

            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> AutocompleteRoles(string query, [FromQuery] bool rankOnly)
        {
            if (query.StartsWith('@'))
            {
                query = query.Substring(1);
            }

            if (rankOnly)
            {
                var criteria = new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = UserGuild.Id,
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
                IEnumerable<IRole> result = UserGuild.Roles;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    result = result.Where(d => d.Name.OrdinalContains(query));
                }

                return Ok(result.Take(10).Select(d => new { d.Id, d.Name }));
            }
        }
    }
}
