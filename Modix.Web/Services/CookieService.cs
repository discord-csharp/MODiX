using Microsoft.JSInterop;
using Modix.Web.Models;

namespace Modix.Web.Services;

public class CookieService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SessionState _sessionState;

    public CookieService(IJSRuntime jsRuntime, SessionState sessionState)
    {
        _jsRuntime = jsRuntime;
        _sessionState = sessionState;
    }

    public async Task SetSelectedGuildAsync(ulong guildId)
    {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{CookieConstants.SelectedGuild}={guildId}; path=/\";");
        _sessionState.SelectedGuild = guildId;
    }
}
