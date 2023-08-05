using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Modix.Web.Services;

public class LocalStorageService
{
    private readonly ProtectedLocalStorage _localStorage;

    public LocalStorageService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<bool> GetBoolAsync(string key)
    {
        var result = await _localStorage.GetAsync<bool>(key);
        return result.Value;
    }

    public async Task SetBoolAsync(string key, bool value)
    {
        await _localStorage.SetAsync(key, value);
    }
}
