using System;
using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models.Core;
using Serilog;

namespace Modix
{
    public class Program
    {
        public static int Main(string[] args)
        {
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

            if (string.IsNullOrWhiteSpace(config.DiscordToken))
            {
                Log.Fatal("The discord token was not set - this is fatal! Check your envvars.");
            }

            if (string.IsNullOrWhiteSpace(config.DiscordClientId) || string.IsNullOrWhiteSpace(config.DiscordClientSecret))
            {
                Log.Warning("The discord client id and/or client secret were not set. These are required for Web API functionality - " +
                    "if you need that, set your envvars, and make sure to configure redirect URIs");
            }

            if (int.TryParse(Environment.GetEnvironmentVariable("DiscordMessageCacheSize"), out var cacheSize))
            {
                config.MessageCacheSize = cacheSize;
            }
            else
            {
                config.MessageCacheSize = 0;
                Log.Information("The message cache size was not set. Defaulting to 0.");
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
