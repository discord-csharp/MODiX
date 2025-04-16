using System.Security.Claims;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Services.Core;
using Modix.Services.GuildStats;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Stats;

namespace Modix.Web.Controllers;

[Route("~/api")]
[ApiController]
[Authorize]
public class GuildStatsController : ModixController
{
    private readonly IGuildStatService _guildStatService;

    public GuildStatsController(IGuildStatService guildStatService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _guildStatService = guildStatService;
    }

    [HttpGet("guildstats")]
    public async Task<GuildStatData> GuildStatsAsync()
    {
        var roleCounts = await _guildStatService.GetGuildMemberDistributionAsync(UserGuild);
        var messageCounts = await _guildStatService.GetTopMessageCounts(UserGuild, SocketUser.Id);

        var guildRoleCounts = roleCounts.Select(x => new GuildRoleMemberCount(x.Name, x.Count, x.Color));
        var topUserMessageCounts = messageCounts.Select(x => new PerUserMessageCount(x.Username, x.Discriminator, x.Rank, x.MessageCount, x.IsCurrentUser));

        return new GuildStatData(UserGuild.Name, [..guildRoleCounts], [..topUserMessageCounts]);
    }
}
