using Discord.WebSocket;
using Microsoft.JSInterop;
using Modix.Web.Models;

namespace Modix.Web.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly DiscordSocketClient _client;
    private readonly SessionState _sessionState;
    public const string GuildCookieKey = "SelectedGuild";

    public LocalStorageService(IJSRuntime jsRuntime, DiscordSocketClient client, SessionState sessionState)
    {
        _jsRuntime = jsRuntime;
        _client = client;
        _sessionState = sessionState;
    }

    public async Task SetSelectedGuildAsync(ulong guildId)
    {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{GuildCookieKey}={guildId}\"");
        _sessionState.SelectedGuild = guildId;
    }
}
