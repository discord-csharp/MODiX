using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models.Core;
using Modix.Services;
using Modix.Web.Shared.Models.Configuration;

namespace Modix.Web.Controllers;

[Route("~/api/config/channels")]
[ApiController]
[Authorize]
public class DesignatedChannelController : ModixController
{
    private readonly DesignatedChannelService _designatedChannelService;

    public DesignatedChannelController(DesignatedChannelService designatedChannelService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _designatedChannelService = designatedChannelService;
    }


    [HttpGet]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedChannelMappingRead))]
    public async Task<Dictionary<Shared.Models.Configuration.DesignatedChannelType, List<DesignatedChannelData>>> GetChannelDesignationsAsync()
    {
        var designatedChannels = await _designatedChannelService.GetDesignatedChannels(UserGuild.Id);

        return designatedChannels
            .Select(d => new DesignatedChannelData(
                d.Id,
                d.Channel.Id,
                (Shared.Models.Configuration.DesignatedChannelType)(int)d.Type,
                UserGuild?.GetChannel(d.Channel.Id)?.Name ?? d.Channel.Name))
            .ToLookup(x => x.ChannelDesignation, x => x)
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    [HttpPut("{channelId}/{designatedChannelType}")]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedChannelMappingCreate))]
    public async Task<IActionResult> CreateDesignationAsync(ulong channelId, Shared.Models.Configuration.DesignatedChannelType designatedChannelType)
    {
        var foundChannel = UserGuild?.GetChannel(channelId);

        if (foundChannel is not ISocketMessageChannel messageChannel)
            return BadRequest($"A message channel was not found with id {channelId} in guild with id {UserGuild.Id}");

        var id = await _designatedChannelService.AddDesignatedChannel(foundChannel.Guild, messageChannel, (Data.Models.Core.DesignatedChannelType)(int)designatedChannelType);

        return Ok(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(AuthorizationClaim.DesignatedChannelMappingDelete))]
    public async Task RemoveDesignationAsync(long id)
    {
        await _designatedChannelService.RemoveDesignatedChannelById(id);
    }
}
