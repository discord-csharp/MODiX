using Discord;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Web.Models;
using Modix.Web.Models.UserLookup;

namespace Modix.Web.Services;

public class DiscordUserService
{
    private readonly DiscordSocketClient _client;
    private readonly IUserService _userService;
    private readonly SessionState _sessionState;

    public DiscordUserService(
        DiscordSocketClient client,
        IUserService userService,
        SessionState sessionState)
    {
        _client = client;
        _userService = userService;
        _sessionState = sessionState;
    }

    public SocketGuild GetUserGuild()
    {
        if (_sessionState.SelectedGuild != 0)
            return _client.GetGuild(_sessionState.SelectedGuild);

        return _client.Guilds.First();
    }

    public IEnumerable<GuildOption> GetGuildOptionsAsync()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
            return Array.Empty<GuildOption>();

        return _client
            .Guilds
            .Where(d => d.GetUser(currentUser.Id) != null)
            .Select(d => new GuildOption(d.Id, d.Name, d.IconUrl));
    }

    public SocketGuildUser? GetCurrentUser()
    {
        var currentGuild = GetUserGuild();
        return currentGuild.GetUser(_sessionState.CurrentUserId);
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

    public IEnumerable<RoleInformation> AutoCompleteRoles(string query)
    {
        if (query.StartsWith('@'))
        {
            query = query[1..];
        }

        var currentGuild = GetUserGuild();
        IEnumerable<IRole> result = currentGuild.Roles;

        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return result.Take(10).Select(d => new RoleInformation(d.Id, d.Name, d.Color.ToString()));
    }
}
