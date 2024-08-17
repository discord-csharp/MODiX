using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Modix.Web.Models;
using Modix.Web.Wasm.Security;
using MudBlazor;
using MudBlazor.Services;

namespace Modix.Web.Wasm;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddScoped<SessionState>();

        builder.Services.AddCascadingValue(sp => sp.GetRequiredService<SessionState>());

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

        builder.Services
            .AddMudServices()
            .AddMudMarkdownServices();

        await builder.Build().RunAsync();
    }
}
