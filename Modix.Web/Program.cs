using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.CodePaste;
using Modix.Services.Utilities;
using Modix.Web.Models;
using Modix.Web.Security;
using Modix.Web.Services;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Modix.Web;

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

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Verbose()
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

        var seqEndpoint = config.SeqEndpoint;
        var seqKey = config.SeqKey;

        if (seqEndpoint != null && seqKey == null) // seq is enabled without a key
        {
            loggerConfig = loggerConfig.WriteTo.Seq(seqEndpoint);
        }
        else if (seqEndpoint != null && seqKey != null) //seq is enabled with a key
        {
            loggerConfig = loggerConfig.WriteTo.Seq(seqEndpoint, apiKey: seqKey);
        }

        var webhookId = config.LogWebhookId;
        var webhookToken = config.LogWebhookToken;

        //.ConfigureWebHostDefaults(webBuilder =>
        //{
        //    webBuilder
        //        .UseConfiguration(builtConfig)
        //        .UseStartup<Startup>();
        //})
        //.UseSerilog();

        var host = builder.Build();

        if (!host.Environment.IsDevelopment())
        {
            host.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            host.UseHsts();
        }

        host.UseHttpsRedirection();
        host.UseStaticFiles();

        host.UseRouting();

        host.UseRequestLocalization("en-US");

        host.UseResponseCompression();
        host.UseAuthentication();
        host.UseMiddleware<ClaimsMiddleware>();

        host.UseAuthorization();

        host.MapGet("/login", async (context) => await context.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));
        host.MapGet("/logout", async (context) => await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));

        host.Map("/invite", builder =>
        {
            builder.Run(handler =>
            {
                //TODO: Maybe un-hardcode this?
                //handler.Response.StatusCode = StatusCodes

                handler.Response.Redirect("https://aka.ms/csharp-discord");
                return Task.CompletedTask;
            });
        });

        // TODO: Is this important? Possibly remove
        //host.UseMvcWithDefaultRoute();
        host.MapBlazorHub();
        host.MapFallbackToPage("/_Host");

        if (webhookId.HasValue && webhookToken != null)
        {
            loggerConfig = loggerConfig
                .WriteTo.DiscordWebhookSink(webhookId.Value, webhookToken, LogEventLevel.Error, host.Services.GetRequiredService<CodePasteService>());
        }

        Log.Logger = loggerConfig.CreateLogger();

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
        builder.Services.AddScoped<DiscordHelper>();
        builder.Services.AddScoped<CookieService>();
        builder.Services.AddScoped<SessionState>();
        builder.Services.AddScoped<LocalStorageService>();
        builder.Services.AddMudServices();
        builder.Services.AddMudMarkdownServices();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddServices(Assembly.GetExecutingAssembly(), configuration);

        builder.Services.Configure<ModixConfig>(configuration);

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(@"dataprotection"));

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddDiscord(options =>
            {
                options.ClientId = modixConfig.DiscordClientId!;
                options.ClientSecret = modixConfig.DiscordClientSecret!;
                options.SaveTokens = true;
                options.AccessDeniedPath = "/";

                options.Scope.Add("identify");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.UInteger64);
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username", ClaimValueTypes.String);
            });

        builder.Services.AddAuthorization(config =>
        {
            var claims = Enum.GetValues<AuthorizationClaim>();
            foreach (var claim in claims)
            {
                config.AddPolicy(claim.ToString(), builder => builder.RequireClaim(ClaimTypes.Role, claim.ToString()));
            }
        });

        //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        //    .AddCookie(options =>
        //    {
        //        options.LoginPath = "/api/unauthorized";
        //        //options.LogoutPath = "/logout";
        //        options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
        //    })
        //    .AddDiscordAuthentication();

        builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        builder.Services.AddResponseCompression();

        // TODO: Fix this
        //services.AddTransient<IConfigureOptions<StaticFileOptions>, StaticFilesConfiguration>();
        //services.AddTransient<IStartupFilter, ModixConfigValidator>();

        builder.Services
            .AddServices(typeof(ModixContext).Assembly, configuration)
            .AddNpgsql<ModixContext>(configuration.GetValue<string>(nameof(ModixConfig.DbConnection)));

        builder.Services
            .AddModixHttpClients()
            .AddModix(configuration);

        builder.Services.AddMvc(d => d.EnableEndpointRouting = false);
    }
}
