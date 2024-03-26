﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Modix.Authentication;
using Modix.Configuration;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.CodePaste;
using Modix.Services.Utilities;
using Modix.Web;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Modix
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configBuilder = builder.Configuration
                .AddEnvironmentVariables("MODIX_")
                .AddJsonFile("developmentSettings.json", optional: true, reloadOnChange: false)
                .AddKeyPerFile("/run/secrets", true);

            if (builder.Environment.IsDevelopment())
            {
                configBuilder.AddUserSecrets<Program>();
            }

            var builtConfig = configBuilder.Build();
            var config = new ModixConfig();
            builtConfig.Bind(config);

            ConfigureServices(builder, builtConfig, config);

            if (config.UseBlazor)
            {
                builder.Services.ConfigureBlazorServices();
            }
            else
            {
                ConfigureVueServices(builder.Services);
            }

            var host = builder.Build();

            ConfigureCommon(host);

            if (config.UseBlazor)
            {
                host.ConfigureBlazorApplication();
            }
            else
            {
                ConfigureVueApplication(host);
            }

            try
            {
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.ForContext<Program>()
                    .Fatal(ex, "Host terminated unexpectedly.");

                if (Debugger.IsAttached && Environment.UserInteractive)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to exit...");
                    Console.ReadKey(true);
                }

                return ex.HResult;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(WebApplicationBuilder builder, IConfiguration configuration, ModixConfig modixConfig)
        {
            builder.Host.UseSerilog((ctx, sp, lc) =>
            {
                lc.MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Modix.DiscordSerilogAdapter", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Logger(subLoggerConfig => subLoggerConfig
                    .MinimumLevel.Information()
                    // .MinimumLevel.Override() is not supported for sub-loggers, even though the docs don't specify this. See https://github.com/serilog/serilog/pull/1033
                    .Filter.ByExcluding("SourceContext like 'Microsoft.%' and @l in ['Information', 'Debug', 'Verbose']")
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", "{Date}.log"), rollingInterval: RollingInterval.Day))
                .WriteTo.File(
                    new RenderedCompactJsonFormatter(),
                    Path.Combine("logs", "{Date}.clef"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 2);

                var seqEndpoint = modixConfig.SeqEndpoint;
                var seqKey = modixConfig.SeqKey;

                if (seqEndpoint != null && seqKey == null) // seq is enabled without a key
                {
                    lc.WriteTo.Seq(seqEndpoint);
                }
                else if (seqEndpoint != null && seqKey != null) //seq is enabled with a key
                {
                    lc.WriteTo.Seq(seqEndpoint, apiKey: seqKey);
                }

                var webhookId = modixConfig.LogWebhookId;
                var webhookToken = modixConfig.LogWebhookToken;
                if (webhookId.HasValue && webhookToken != null)
                {
                    lc
                        .WriteTo.DiscordWebhookSink(webhookId.Value, webhookToken, LogEventLevel.Error, new Lazy<CodePasteService>(sp.GetRequiredService<CodePasteService>));
                }
            });

            builder.Services.AddServices(Assembly.GetExecutingAssembly(), configuration);

            builder.Services.Configure<ModixConfig>(configuration);

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"dataprotection"));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/api/unauthorized";
                    //options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
                })
                .AddDiscordAuthentication();

            builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            builder.Services.AddResponseCompression();

            builder.Services.AddTransient<IConfigureOptions<StaticFileOptions>, StaticFilesConfiguration>();
            builder.Services.AddTransient<IStartupFilter, ModixConfigValidator>();

            builder.Services
                .AddServices(typeof(ModixContext).Assembly, configuration)
                .AddNpgsql<ModixContext>(configuration.GetValue<string>(nameof(ModixConfig.DbConnection)));

            builder.Services
                .AddModixHttpClients()
                .AddModix(configuration);

            builder.Services.AddMvc(d => d.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new StringULongConverter());
                });
        }

        public static void ConfigureVueServices(IServiceCollection services)
        {
            services.AddMvc(d => d.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new StringULongConverter());
                });
        }

        public static void ConfigureVueApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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

        public static void ConfigureCommon(WebApplication app)
        {
            const string logFilesRequestPath = "/logfiles";

            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            app.UseForwardedHeaders(options);

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

            // Serve up log files for maintainers only
            app.MapWhen(x => x.Request.Path.Value.StartsWith(logFilesRequestPath), builder =>
            {
                var fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
                builder
                    .UseMiddleware<LogFilesAuthorizationMiddleware>()
                    .UseDirectoryBrowser(new DirectoryBrowserOptions()
                    {
                        FileProvider = fileProvider,
                        RequestPath = logFilesRequestPath
                    })
                    .UseStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = fileProvider,
                        RequestPath = logFilesRequestPath,
                        ServeUnknownFileTypes = true
                    });
            });
        }
    }
}
