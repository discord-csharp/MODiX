using System;
using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models.Core;
using Modix.Services.Utilities;
using Serilog;
using Serilog.Events;

namespace Modix
{
    public class Program
    {
        public static int Main(string[] args)
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
                loggerConfig.WriteTo.DiscordWebhookSink(id, webhookToken, LogEventLevel.Error);
            }

            if (!string.IsNullOrWhiteSpace(sentryToken))
            {
                loggerConfig.WriteTo.Sentry(sentryToken, restrictedToMinimumLevel: LogEventLevel.Warning);
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                CreateWebHostBuilder(args).Build().Run();

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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(services => services.AddSingleton(LoadConfig()))
                .UseStartup<Startup>();

        private static ModixConfig LoadConfig()
        {
            var config = new ModixConfig
            {
                DiscordToken = Environment.GetEnvironmentVariable("DiscordToken"),
                ReplToken = Environment.GetEnvironmentVariable("ReplToken"),
                StackoverflowToken = Environment.GetEnvironmentVariable("StackoverflowToken"),
                PostgreConnectionString = Environment.GetEnvironmentVariable("MODIX_DB_CONNECTION"),
                DiscordClientId = Environment.GetEnvironmentVariable("DiscordClientId"),
                DiscordClientSecret = Environment.GetEnvironmentVariable("DiscordClientSecret")
            };

            if (int.TryParse(Environment.GetEnvironmentVariable("DiscordMessageCacheSize"), out var cacheSize))
            {
                config.MessageCacheSize = cacheSize;
            }

            var id = Environment.GetEnvironmentVariable("log_webhook_id");

            if (!string.IsNullOrWhiteSpace(id))
            {
                config.WebhookId = ulong.Parse(id);
                config.WebhookToken = Environment.GetEnvironmentVariable("log_webhook_token");
            }

            var sentryToken = Environment.GetEnvironmentVariable("SentryToken");
            if (!string.IsNullOrWhiteSpace(sentryToken))
            {
                config.SentryToken = sentryToken;
            }

            return config;
        }
    }
}
