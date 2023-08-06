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
        await SetCookieAsync(CookieConstants.SelectedGuild, guildId);
        _sessionState.SelectedGuild = guildId;
    }

    public async Task SetShowDeletedInfractionsAsync(bool showDeleted)
    {
        await SetCookieAsync(CookieConstants.ShowDeletedInfractions, showDeleted);
        _sessionState.ShowDeletedInfractions = showDeleted;
    }

    public async Task SetShowInfractionStateAsync(bool showInfractionState)
    {
        await SetCookieAsync(CookieConstants.ShowInfractionState, showInfractionState);
        _sessionState.ShowInfractionState = showInfractionState;
    }

    public async Task SetShowInactivePromotionsAsync(bool showInactivePromotions)
    {
        await SetCookieAsync(CookieConstants.ShowInactivePromotions, showInactivePromotions);
        _sessionState.ShowInactivePromotions = showInactivePromotions;
    }

    private async Task SetCookieAsync<T>(string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{key}={value}; path=/\";");
    }
}
