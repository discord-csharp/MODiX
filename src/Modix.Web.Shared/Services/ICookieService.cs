namespace Modix.Web.Shared.Services;

public interface ICookieService
{
    Task SetSelectedGuildAsync(ulong guildId);
    Task SetShowDeletedInfractionsAsync(bool showDeleted);
    Task SetShowInfractionStateAsync(bool showInfractionState);
    Task SetShowInactivePromotionsAsync(bool showInactivePromotions);
    Task SetUseDarkModeAsync(bool useDarkMode);
}
