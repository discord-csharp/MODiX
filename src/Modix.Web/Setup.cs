﻿using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Modix.Web.Models;
using Modix.Web.Security;
using Modix.Web.Services;
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

        app.MapGet("/login", async (context) => await context.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));
        app.MapGet("/logout", async (context) => await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
            //.AddInteractiveWebAssemblyRenderMode();
        //TODO
        //.AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        return app;
    }

    public static IServiceCollection ConfigureBlazorServices(this IServiceCollection services)
    {
        services
            .AddScoped<DiscordHelper>()
            .AddScoped<CookieService>()
            .AddScoped<SessionState>()
            .AddMudServices()
            .AddMudMarkdownServices();

        services.AddCascadingAuthenticationState();

        services
            .AddRazorComponents()
            .AddInteractiveServerComponents();
            //.AddInteractiveWebAssemblyComponents();

        return services;
    }
}
