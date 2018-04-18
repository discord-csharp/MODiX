using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;
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

        public ApiController(DiscordSocketClient client, GuildInfoService guildInfoService) : base(client)
        {
            _guildInfoService = guildInfoService;
        }

        public async Task<IActionResult> Guilds()
        {
            var guildInfo = new Dictionary<string, List<GuildInfoResult>>();

            foreach (var guild in _client.Guilds)
            {
                guildInfo.Add(guild.Name, (await _guildInfoService.GetGuildMemberDistribution(guild)));
            }

            return Ok(guildInfo);
        }

        public IActionResult UserInfo()
        {
            return Ok(DiscordUser);
        }

        public async Task<IActionResult> Autocomplete(string query)
        {
            await _client.Guilds.First().DownloadUsersAsync();

            var result = _client.Guilds.First()
                .Users.Where(d => d.Username.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) > 0)
                .Take(10)
                .Select(d => new DiscordUser { Name = $"{d.Username}#{d.Discriminator}", UserId = d.Id, AvatarHash = d.AvatarId });

            return Ok(result);
        }
    }
}
