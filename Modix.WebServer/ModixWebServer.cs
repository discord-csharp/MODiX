using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models.Core;
using Modix.WebServer.Auth;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modix.WebServer
{
    public class ModixWebServer
    {
        private static IServiceCollection _additionalServices;
        private static ModixConfig _modixConfig;
        private readonly bool _isProduction;

        public ModixWebServer(IHostingEnvironment env)
        {
            _isProduction = !env.IsDevelopment();
        }

        // Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var service in _additionalServices)
            {
                services.Add(service);
            }

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"dataprotection"));

            services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/api/unauthorized";
                //options.LogoutPath = "/logout";
                options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);

            })
            .AddModix(_modixConfig);

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddResponseCompression();

            services
            .AddMvc()
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.Converters.Add(new StringULongConverter());
            });
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IAntiforgery antiforgery)
        {
            app.UseAuthentication();
            app.UseResponseCompression();

            //Static redirect for invite link
            app.MapWhen(x => x.Request.Path.Value.StartsWith("/invite"), builder =>
            {
                builder.Run(handler =>
                {
                    //TODO: Maybe un-hardcode this?
                    handler.Response.Redirect("https://aka.ms/csharp-discord");
                    return Task.CompletedTask;
                });
            });

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
