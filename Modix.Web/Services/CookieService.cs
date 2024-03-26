using Microsoft.JSInterop;
using Modix.Web.Models;

namespace Modix.Web.Services;

public class CookieService(IJSRuntime jsRuntime, SessionState sessionState)
{
    public async Task SetSelectedGuildAsync(ulong guildId)
    {
        await SetCookieAsync(CookieConstants.SelectedGuild, guildId);
        sessionState.SelectedGuild = guildId;
    }

    public async Task SetShowDeletedInfractionsAsync(bool showDeleted)
    {
        await SetCookieAsync(CookieConstants.ShowDeletedInfractions, showDeleted);
        sessionState.ShowDeletedInfractions = showDeleted;
    }

    public async Task SetShowInfractionStateAsync(bool showInfractionState)
    {
        await SetCookieAsync(CookieConstants.ShowInfractionState, showInfractionState);
        sessionState.ShowInfractionState = showInfractionState;
    }

    public async Task SetShowInactivePromotionsAsync(bool showInactivePromotions)
    {
        await SetCookieAsync(CookieConstants.ShowInactivePromotions, showInactivePromotions);
        sessionState.ShowInactivePromotions = showInactivePromotions;
    }

    public async Task SetUseDarkModeAsync(bool useDarkMode)
    {
        await SetCookieAsync(CookieConstants.UseDarkMode, useDarkMode);
        sessionState.UseDarkMode = useDarkMode;
    }

    private async Task SetCookieAsync<T>(string key, T value)
        => await jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{key}={value}; path=/\";");
}
