using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Models;
using Modix.Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modix.Controllers
{
    public class ApiController : ModixController
    {
        public ApiController(DiscordSocketClient client, IAuthorizationService auth) : base(client, auth)
        {
            
        }

        public IActionResult Roles()
        {
            return Ok(UserGuild.Roles.Select(d => new { d.Id, d.Name, Color = d.Color.ToString() }));
        }

        public IActionResult RolesExceptEveryone()
        {
            return Ok(UserGuild.Roles.Where(d => !d.IsEveryone).Select(d => new { d.Id, d.Name, Color = d.Color.ToString(), d.Position, d.IsMentionable }));
        }

        public IActionResult RolesExceptEveryone()
        {
            return Ok(UserGuild.Roles.Where(d => !d.IsEveryone).Select(d => new { d.Id, d.Name, Color = d.Color.ToString(), d.Position, d.IsMentionable }));
        }

        public IActionResult Channels()
        {
            return Ok(UserGuild.Channels.Select(d => new { d.Id, d.Name }));
        }

        public IActionResult Claims()
        {
            return Ok(ClaimInfoData.GetClaims());
        }

        [HttpGet("~/api/me")]
        public IActionResult LoggedInUserInfo()
        {
            return Ok(ModixUser);
        }

        [HttpGet]
        public IActionResult GuildOptions()
        {
            var guilds = DiscordSocketClient
                .Guilds
                .Where(d => d.GetUser(SocketUser.Id) != null)
                .Select(d => new { d.Name, d.Id, d.IconUrl });

            return Ok(guilds);
        }

        [HttpPost("~/api/switchGuild/{guildId}")]
        public IActionResult SwitchGuild(ulong guildId)
        {
            var user = DiscordSocketClient.GetGuild(guildId)?.GetUser(SocketUser.Id);

            if (user == null)
            {
                return BadRequest("Invalid guild, or user is not a member of the guild.");
            }

            Response.Cookies.Append("SelectedGuild", user.Guild.Id.ToString());

            return Ok();
        }
    }
}
