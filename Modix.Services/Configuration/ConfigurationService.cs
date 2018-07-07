using System;
using Nett;

namespace Modix.Services.Configuration
{
    public interface IConfigurationService
    {
        DiscordBotConfiguration LoadDiscordBotConfiguration();
    }

    public class ConfigurationService : IConfigurationService
    {
        private const string DiscordBotConfigurationPath = "configuration.toml";

        public DiscordBotConfiguration LoadDiscordBotConfiguration()
        {
            return Toml.ReadFile<DiscordBotConfiguration>(DiscordBotConfigurationPath);
        }
    }
}