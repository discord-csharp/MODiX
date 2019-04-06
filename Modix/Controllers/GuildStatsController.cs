using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.GuildStats;

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
            var messageCounts = await _statService.GetTopMessageCounts(UserGuild, ModixUser.UserId);

            return Ok(new GuildStatApiData
            {
                GuildName = UserGuild.Name,
                GuildRoleCounts = roleCounts,
                TopUserMessageCounts = messageCounts,
            });
        }
    }
}
