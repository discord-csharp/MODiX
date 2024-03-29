using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;
using Serilog;

namespace Modix.Configuration
{
    public class ModixConfigValidator : IStartupFilter
    {
        private readonly ModixConfig _config;

        public ModixConfigValidator(IOptions<ModixConfig> config)
        {
            _config = config.Value;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (string.IsNullOrWhiteSpace(_config.DbConnection))
            {
                Log.Fatal("The db connection string was not set - this is fatal! Check your config.");
            }

            if (string.IsNullOrWhiteSpace(_config.DiscordToken))
            {
                Log.Fatal("The discord token was not set - this is fatal! Check your config.");
            }

            if (string.IsNullOrWhiteSpace(_config.DiscordClientId) || string.IsNullOrWhiteSpace(_config.DiscordClientSecret))
            {
                Log.Warning("The discord client id and/or client secret were not set. These are required for Web API functionality - " +
                    "if you need that, set your config, and make sure to configure redirect URIs");
            }

            if (_config.LogWebhookId == default || string.IsNullOrWhiteSpace(_config.LogWebhookToken))
            {
                Log.Warning("The log webhook ID and/or token were not set. Errors will not be logged to Discord.");
            }

            Log.Information("The message cache size is {Default}.", _config.MessageCacheSize);

            return next;
        }
    }
}
