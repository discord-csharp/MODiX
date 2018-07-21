using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Modix.Services.Utilities;
using Serilog;
using Serilog.Events;

namespace Modix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = LoadConfig();
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Discord.WebSocket", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(@"logs\{Date}", restrictedToMinimumLevel: LogEventLevel.Debug);

            if (!string.IsNullOrWhiteSpace(config.WebhookToken))
            {
                loggerConfig.WriteTo.DiscordWebhookSink(config.WebhookId, config.WebhookToken, LogEventLevel.Error);
            }

            if (!string.IsNullOrWhiteSpace(config.SentryToken))
            {
                loggerConfig.WriteTo.Sentry(config.SentryToken, restrictedToMinimumLevel: LogEventLevel.Warning);
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                await new ModixBot(config, Log.Logger).Run();
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

                Environment.Exit(ex.HResult);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static ModixConfig LoadConfig()
        {
            var config = new ModixConfig
            {
                DiscordToken = Environment.GetEnvironmentVariable("DiscordToken"),
                ReplToken = Environment.GetEnvironmentVariable("ReplToken"),
                StackoverflowToken = Environment.GetEnvironmentVariable("StackoverflowToken"),
                PostgreConnectionString = Environment.GetEnvironmentVariable("MODIX_DB_CONNECTION"),
                DiscordClientId = Environment.GetEnvironmentVariable("DiscordClientId"),
                DiscordClientSecret = Environment.GetEnvironmentVariable("DiscordClientSecret"),
            };

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