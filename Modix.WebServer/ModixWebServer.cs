using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models;
using Modix.Services.CodePaste;
using Modix.Services.GuildInfo;
using System;
using System.IO;

namespace Modix.WebServer
{
    public class ModixWebServer
    {
        private static IServiceCollection _additionalServices;
        private static ModixConfig _modixConfig;

        // Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var service in _additionalServices)
            {
                services.Add(service);
            }

            services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/api/unauthorized";
                //options.LogoutPath = "/logout";
                options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
                
            })
            .AddDiscord(options =>
            {
                options.ClaimActions.MapJsonKey(claimType: "avatarHash", jsonKey: "avatar");

                options.ClientId = _modixConfig.DiscordClientId;
                options.ClientSecret = _modixConfig.DiscordClientSecret;
                options.Scope.Add("identify");
            });

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddMvc();
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IAntiforgery antiforgery)
        {
            app.UseAuthentication();

            //Map to static files when not hitting the API
            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            {
                //Tiny middleware to redirect invalid requests to index.html,
                //this ensures that our frontend routing works on fresh requests
                builder.Use(async (context, next) =>
                {
                    await next();
                    if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
                    {
                        context.Request.Path = "/index.html";
                        await next();
                    }
                })
                .UseDefaultFiles()
                .UseStaticFiles(new StaticFileOptions
                {
                    //Set up our antiforgery stuff when the user hits the page
                    OnPrepareResponse = fileResponse =>
                    {
                        if (fileResponse.File.Name == "index.html")
                        {
                            var tokens = antiforgery.GetAndStoreTokens(fileResponse.Context);

                            fileResponse.Context.Response.Cookies.Append(
                                "XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                        }
                    }
                });
            });

            //Defer to MVC for anything that doesn't match (and ostensibly 
            //starts with /api)
            app.UseMvcWithDefaultRoute();
        }

        public static IWebHost BuildWebHost(IServiceCollection col, ModixConfig modixConfig)
        {
            _additionalServices = col;
            _modixConfig = modixConfig;

            var host = WebHost.CreateDefaultBuilder()
                              .UseStartup<ModixWebServer>()
                              .Build();

            return host;
        }
    }
}
