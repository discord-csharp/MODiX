using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/autocomplete")]
[ApiController]
[Authorize]
public class AutocompleteController : ModixController
{
    private readonly IUserService _userService;

    public AutocompleteController(DiscordSocketClient discordSocketClient, IUserService userService, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _userService = userService;
    }


    [HttpGet("users/{query}")]
    public async Task<IEnumerable<ModixUser>> AutocompleteUsersAsync(string query)
    {
        var result = UserGuild.Users
                .Where(d => d.Username.OrdinalContains(query) || d.Id.ToString() == query)
                .Take(10)
                .Select(FromIGuildUser);

        if (!result.Any() && ulong.TryParse(query, out var userId))
        {
            var user = await _userService.GetUserInformationAsync(UserGuild.Id, userId);

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
