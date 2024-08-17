using System.Security.Claims;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.GuildStats;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Stats;

namespace Modix.Web.Controllers;

[Route("~/api")]
[ApiController]
[Authorize]
public class GuildStatsController : ControllerBase
{
    private readonly IGuildStatService _guildStatService;
    private readonly DiscordSocketClient _discordSocketClient;

    public GuildStatsController(IGuildStatService guildStatService, DiscordSocketClient discordSocketClient)
    {
        _guildStatService = guildStatService;
        _discordSocketClient = discordSocketClient;
    }

    [HttpGet("guildstats")]
    public async Task<GuildStatData> GuildStats()
    {
        // TODO: Move this to a base class like ModixController?
        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

        SocketGuild guildToSearch;
        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = _discordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = _discordSocketClient.Guilds.First();
        }


        var userId = ulong.Parse(User.FindFirst(d => d.Type == ClaimTypes.NameIdentifier)?.Value);

        var roleCounts = await _guildStatService.GetGuildMemberDistributionAsync(guildToSearch);
        var messageCounts = await _guildStatService.GetTopMessageCounts(guildToSearch, userId);

        var guildRoleCounts = roleCounts.Select(x => new GuildRoleMemberCount(x.Name, x.Count, x.Color));
        var topUserMessageCounts = messageCounts.Select(x => new PerUserMessageCount(x.Username, x.Discriminator, x.Rank, x.MessageCount, x.IsCurrentUser));

        return new GuildStatData(guildToSearch.Name, [..guildRoleCounts], [..topUserMessageCounts]);
    }
}
