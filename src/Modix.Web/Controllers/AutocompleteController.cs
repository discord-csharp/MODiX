using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/autocomplete")]
[ApiController]
[Authorize]
public class AutocompleteController : ControllerBase
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IUserService _userService;

    public AutocompleteController(DiscordSocketClient discordSocketClient, IUserService userService)
    {
        _discordSocketClient = discordSocketClient;
        _userService = userService;
    }


    [HttpGet("users/{query}")]
    public async Task<IEnumerable<ModixUser>> AutocompleteUsers(string query)
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

        if (guildToSearch?.Users is null)
            return [];

        var result = guildToSearch.Users
                .Where(d => d.Username.OrdinalContains(query) || d.Id.ToString() == query)
                .Take(10)
                .Select(FromIGuildUser);

        if (!result.Any() && ulong.TryParse(query, out var userId))
        {
            var user = await _userService.GetUserInformationAsync(guildToSearch.Id, userId);

            if (user is not null)
            {
                result = [ FromNonGuildUser(user) ];
            }
        }

        return result;
    }


    public static ModixUser FromIGuildUser(IGuildUser user) => new()
    {
        Name = user.GetDisplayName(),
        UserId = user.Id,
        AvatarUrl = user.GetDisplayAvatarUrl() ?? user.GetDefaultAvatarUrl()
    };

    public static ModixUser FromNonGuildUser(IUser user) => new()
    {
        Name = user.GetDisplayName(),
        UserId = user.Id,
        AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
    };
}
