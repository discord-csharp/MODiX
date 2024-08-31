using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/autocomplete")]
[ApiController]
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

        if (result.Any() || !ulong.TryParse(query, out var userId))
            return result;

        var user = await _userService.GetUserInformationAsync(UserGuild.Id, userId);

        if (user is not null)
            return [ FromNonGuildUser(user) ];

        return [];
    }

    [HttpGet("channels/{query}")]
    public IEnumerable<ChannelInformation> AutoCompleteChannels(string query)
    {
        if (query.StartsWith('#'))
        {
            query = query[1..];
        }

        return UserGuild.Channels
            .Where(d => d is SocketTextChannel
                && d.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(d => new ChannelInformation(d.Id, d.Name));
    }

    [HttpGet("roles/{query}")]
    public IEnumerable<RoleInformation> AutoCompleteRoles(string query)
    {
        if (query.StartsWith('@'))
        {
            query = query[1..];
        }

        IEnumerable<IRole> result = UserGuild.Roles;

        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return result.Take(10).Select(d => new RoleInformation(d.Id, d.Name, d.Color.ToString()));
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
