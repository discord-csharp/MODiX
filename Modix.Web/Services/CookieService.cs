using Microsoft.JSInterop;
using Modix.Web.Models;

namespace Modix.Web.Services;

public class CookieService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SessionState _sessionState;
    public const string GuildCookieKey = "SelectedGuild";

    public CookieService(IJSRuntime jsRuntime, SessionState sessionState)
    {
        _jsRuntime = jsRuntime;
        _sessionState = sessionState;
    }

    public async Task SetSelectedGuildAsync(ulong guildId)
    {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{GuildCookieKey}={guildId}\"");
        _sessionState.SelectedGuild = guildId;
    }
}
