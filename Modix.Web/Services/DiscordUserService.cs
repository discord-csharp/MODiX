using System.Security.Claims;
using Discord.WebSocket;
using Microsoft.AspNetCore.Components.Authorization;
using Modix.Services.Core;
using Modix.Web.Models;

namespace Modix.Web.Services;

public class DiscordUserService
{
    private readonly DiscordSocketClient _client;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserService _userService;
    private readonly SessionState _sessionState;

    public DiscordUserService(
        DiscordSocketClient client,
        AuthenticationStateProvider authenticationStateProvider,
        IUserService userService,
        SessionState sessionState)
    {
        _client = client;
        _authenticationStateProvider = authenticationStateProvider;
        _userService = userService;
        _sessionState = sessionState;
    }

    public SocketGuild GetUserGuild()
    {
        if (_sessionState.SelectedGuild != 0)
            return _client.GetGuild(_sessionState.SelectedGuild);

        return _client.Guilds.First();
    }

    public record GuildOption(ulong Id, string Name, string IconUrl);

    public async Task<IEnumerable<GuildOption>> GetGuildOptionsAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null)
            return Array.Empty<GuildOption>();

        return _client
            .Guilds
            .Where(d => d.GetUser(currentUser.Id) != null)
            .Select(d => new GuildOption(d.Id, d.Name, d.IconUrl));
    }

    public async Task<SocketGuildUser?> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? false)
            return null;

        var userId = authState.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!ulong.TryParse(userId, out var userSnowflake))
            return null;

        var currentGuild = GetUserGuild();
        return currentGuild.GetUser(userSnowflake);
    }

    public async Task<IEnumerable<ModixUser>> AutoCompleteAsync(string query)
    {
        var userGuild = GetUserGuild();

        if (userGuild?.Users is null)
            return Array.Empty<ModixUser>();

        var result = userGuild.Users
                .Where(d => d.Username.Contains(query, StringComparison.OrdinalIgnoreCase) || d.Id.ToString() == query)
                .Take(10)
                .Select(ModixUser.FromIGuildUser);

        if (!result.Any() && ulong.TryParse(query, out var userId))
        {
            var user = await _userService.GetUserInformationAsync(userGuild.Id, userId);

            if (user is not null)
            {
                result = result.Append(ModixUser.FromNonGuildUser(user));
            }
        }

        return result;
    }

}
