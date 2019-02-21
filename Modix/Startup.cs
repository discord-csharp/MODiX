using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modix.Auth;
using Modix.Configuration;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.AutoCodePaste;
using Modix.Services.Utilities;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;

namespace Modix
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            Log.Information("Configuration loaded. ASP.NET Startup is a go.");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ModixConfig>(_configuration);

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"dataprotection"));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/api/unauthorized";
                    //options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);

                })
                .AddModixAuth();

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddResponseCompression();

            services.AddTransient<IConfigureOptions<StaticFileOptions>, StaticFilesConfiguration>();
            services.AddTransient<IStartupFilter, ModixConfigValidator>();

            services.AddDbContext<ModixContext>(options =>
            {
                options.UseNpgsql(_configuration.GetValue<string>("DbConnection"));
            });

            services.AddModix();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new StringULongConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, CodePasteService codePasteService)
        {
            PerformInitialSetup(codePasteService);

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
                .UseStaticFiles();
            });

            //Defer to MVC for anything that doesn't match (and ostensibly
            //starts with /api)
            app.UseMvcWithDefaultRoute();
        }

        public static void PerformInitialSetup(CodePasteService codePasteService)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Modix.DiscordSerilogAdapter", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug);

            var webhookId = Environment.GetEnvironmentVariable("log_webhook_id");
            var webhookToken = Environment.GetEnvironmentVariable("log_webhook_token");
            var sentryToken = Environment.GetEnvironmentVariable("SentryToken");

            if (!string.IsNullOrWhiteSpace(webhookToken) &&
                ulong.TryParse(webhookId, out var id))
            {
                loggerConfig.WriteTo.DiscordWebhookSink(id, webhookToken, LogEventLevel.Error, codePasteService);
            }
            else
            {
                Log.Information("The webhook token and/or ID were not set. Logging to Discord has been disabled.");
            }

            if (!string.IsNullOrWhiteSpace(sentryToken))
            {
                loggerConfig.WriteTo.Sentry(sentryToken, restrictedToMinimumLevel: LogEventLevel.Warning);
            }
            else
            {
                Log.Information("The Sentry token was not set. Logging to Sentry has been disabled.");
            }

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
