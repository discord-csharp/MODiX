using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models.Core;

namespace Modix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
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

            if (int.TryParse(Environment.GetEnvironmentVariable("DiscordMessageCacheSize"), out int cacheSize))
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
