using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using Modix.Services.Mentions;
using Newtonsoft.Json.Converters;
using Serilog;

namespace Modix
{
    public class Startup
    {
        private const string _logFilesRequestPath
            = "/logfiles";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            Log.Information("Configuration loaded. ASP.NET Startup is a go.");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServices(Assembly.GetExecutingAssembly(), _configuration);

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
                .AddDiscordAuthentication();

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddResponseCompression();

            services.AddTransient<IConfigureOptions<StaticFileOptions>, StaticFilesConfiguration>();
            services.AddTransient<IStartupFilter, ModixConfigValidator>();

            services
                .AddServices(typeof(ModixContext).Assembly, _configuration)
                .AddDbContext<ModixContext>(options => options
                    .UseNpgsql(_configuration.GetValue<string>(nameof(ModixConfig.DbConnection)), npgsqlOptions => npgsqlOptions
                        .UseDateTimeOffsetTranslations()));

            services
                .AddModixHttpClients()
                .AddModix(_configuration);

            services.AddMvc(d => d.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new StringULongConverter());
                });

            services.AddStatsD(_webHostEnvironment, _configuration);
            services.AddMediatR(config => config.AsScoped(),typeof(MentionCommand).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, CodePasteService codePasteService)
        {
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            app.UseForwardedHeaders(options);

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

            // Serve up log files for maintainers only
            app.MapWhen(x => x.Request.Path.Value.StartsWith(_logFilesRequestPath), builder =>
            {
                var fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
                builder
                    .UseMiddleware<LogFilesAuthorizationMiddleware>()
                    .UseDirectoryBrowser(new DirectoryBrowserOptions()
                    {
                        FileProvider = fileProvider,
                        RequestPath = _logFilesRequestPath
                    })
                    .UseStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = fileProvider,
                        RequestPath = _logFilesRequestPath,
                        ServeUnknownFileTypes = true
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
    }
}
