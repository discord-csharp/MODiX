using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.GuildInfo;
using Modix.WebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modix.WebServer.Controllers
{
    public class ApiController : ModixController
    {
        private GuildInfoService _guildInfoService;

        public ApiController(DiscordSocketClient client, GuildInfoService guildInfoService, IAuthorizationService auth) : base(client, auth)
        {
            _guildInfoService = guildInfoService;
        }

        public async Task<IActionResult> Guilds()
        {
            var guildInfo = new Dictionary<string, List<GuildInfoResult>>();

            foreach (var guild in DiscordSocketClient.Guilds)
            {
                guildInfo.Add(guild.Name, (await _guildInfoService.GetGuildMemberDistribution(guild)));
            }

            return Ok(guildInfo);
        }

        public IActionResult UserInfo()
        {
            return Ok(ModixUser);
        }

        public async Task<IActionResult> Autocomplete(string query)
        {
            var firstGuild = DiscordSocketClient.Guilds.First();

            await firstGuild.DownloadUsersAsync();

            var result = firstGuild.Users
                .Where(d => d.Username.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(10)
                .Select(d => new ModixUser { Name = $"{d.Username}#{d.Discriminator}", UserId = d.Id, AvatarHash = d.AvatarId });

            return Ok(result);
        }
    }
}
