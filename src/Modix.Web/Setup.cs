using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Modix.Web.Models;
using Modix.Web.Security;
using Modix.Web.Services;
using Modix.Web.Shared.Services;
using MudBlazor;
using MudBlazor.Services;

namespace Modix.Web;

public static class Setup
{
    public static WebApplication ConfigureBlazorApplication(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAntiforgery();

        app.UseRequestLocalization("en-US");
        app.UseMiddleware<ClaimsMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/login", async (context) =>
        {
            await context.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = context.Request.Query.TryGetValue(CookieAuthenticationDefaults.ReturnUrlParameter, out var returnUrl)
                    ? returnUrl.ToString()
                    : "/"
            });
        });
        app.MapGet("/logout", async (context) => await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));

        app.MapRazorComponents<App>()
            //.AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Wasm._Imports).Assembly);

        return app;
    }

    public static IServiceCollection ConfigureBlazorServices(this IServiceCollection services)
    {
        services.AddControllers();

        services
            .AddScoped<DiscordHelper>()
            .AddScoped<ICookieService, CookieService>()
            .AddScoped<SessionState>()
            .AddCascadingValue<SessionState>(sp => sp.GetRequiredService<SessionState>())
            .AddMudServices()
            .AddMudMarkdownServices();

        services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();

        services
            .AddRazorComponents()
            //.AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        return services;
    }
}
