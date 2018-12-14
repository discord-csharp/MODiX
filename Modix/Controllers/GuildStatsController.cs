using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.GuildInfo;

namespace Modix.Controllers
{
    [Route("~/api/guildStats")]
    public class GuildStatsController : ModixController
    {
        private readonly IGuildStatService _statService;

        public GuildStatsController(IGuildStatService statService, DiscordSocketClient client, IAuthorizationService modixAuth) : base(client, modixAuth)
        {
            _statService = statService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roleCounts = await _statService.GetGuildMemberDistributionAsync(UserGuild);
            var messageCounts = await _statService.GetTopMessageCounts(UserGuild);

            return Ok(new GuildStatApiData
            {
                GuildName = UserGuild.Name,
                GuildRoleCounts = roleCounts,
                TopUserMessageCounts = messageCounts.ToDictionary
                (
                    d => $"{d.Key.User.Username}#{d.Key.User.Discriminator}",
                    d => d.Value
                )
            });
        }
    }
}
