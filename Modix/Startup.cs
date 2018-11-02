using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Modix.Auth;
using Modix.Data.Models.Core;
using Newtonsoft.Json.Converters;

namespace Modix
{
    public class Startup
    {
        public Startup(ModixConfig configuration)
        {
            Configuration = configuration;
        }

        public ModixConfig Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"dataprotection"));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/api/unauthorized";
                    //options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);

                })
                .AddModix(Configuration);

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddResponseCompression();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new StringULongConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseResponseCompression();

            //Static redirect for invite link
            app.Map("/invite", builder =>
            {
                builder.Run(handler =>
                {
                    //TODO: Maybe un-hardcode this?
                    //handler.Response.StatusCode = StatusCodes

                    handler.Response.Redirect("https://aka.ms/csharp-discord");
                    return Task.CompletedTask;
                });
            });

            app.UseStaticFiles();

            ////Map to static files when not hitting the API
            //app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            //{
            //    //Tiny middleware to redirect invalid requests to index.html,
            //    //this ensures that our frontend routing works on fresh requests
            //    builder.Use(async (context, next) =>
            //    {
            //        await next();
            //        if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
            //        {
            //            context.Request.Path = "/index.html";
            //            await next();
            //        }
            //    })
            //    .UseDefaultFiles()
            //    .UseStaticFiles(new StaticFileOptions
            //    {
            //        //Set up our antiforgery stuff when the user hits the page
            //        OnPrepareResponse = fileResponse =>
            //        {
            //            if (fileResponse.File.Name == "index.html")
            //            {
            //                var tokens = antiforgery.GetAndStoreTokens(fileResponse.Context);

            //                fileResponse.Context.Response.Cookies.Append(
            //                    "XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
            //            }
            //        }
            //    });
            //});

            ////Defer to MVC for anything that doesn't match (and ostensibly
            ////starts with /api)
            //app.UseMvcWithDefaultRoute();

            app.UseMvc();
        }
    }
}
