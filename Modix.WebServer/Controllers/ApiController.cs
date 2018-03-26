using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.CommandHelp;
using Modix.Services.GuildInfo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modix.WebServer.Controllers
{
    public class ApiController : ModixController
    {
        private DiscordSocketClient _client;
        private GuildInfoService _guildInfoService;
        private CommandHelpService _commandHelpService;

        public ApiController(DiscordSocketClient client, GuildInfoService guildInfoService, CommandHelpService commandHelpService)
        {
            _client = client;
            _guildInfoService = guildInfoService;
            _commandHelpService = commandHelpService;
        }

        [Authorize]
        public async Task<IActionResult> Guilds()
        {
            var guildInfo = new Dictionary<string, List<GuildInfoResult>>();

            foreach (var guild in _client.Guilds)
            {
                guildInfo.Add(guild.Name, (await _guildInfoService.GetGuildMemberDistribution(guild)));
            }

            return Ok(guildInfo);
        }

        [Authorize]
        public IActionResult UserInfo()
        {
            return Ok(DiscordUser);
        }

        public IActionResult Commands()
        {
            return Ok(_commandHelpService.GetData());
        }
    }
}
