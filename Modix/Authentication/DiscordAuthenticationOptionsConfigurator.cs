using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using AspNet.Security.OAuth.Discord;

using Modix.Data.Models.Core;

namespace Modix.Authentication
{
    [ServiceBinding(ServiceLifetime.Transient)]
    public class DiscordAuthenticationOptionsConfigurator
        : IPostConfigureOptions<DiscordAuthenticationOptions>
    {
        public DiscordAuthenticationOptionsConfigurator(
            IOptions<ModixConfig> modixConfig)
        {
            _modixConfig = modixConfig.Value;
        }

        public void PostConfigure(
            string name,
            DiscordAuthenticationOptions options)
        {
            options.ClientId = _modixConfig.DiscordClientId;
            options.ClientSecret = _modixConfig.DiscordClientSecret;
        }

        private readonly ModixConfig _modixConfig;
    }
}
