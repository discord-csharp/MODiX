#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Bot
{
    public static class BotSetup
    {
        public static IServiceCollection AddModixBot(
                this IServiceCollection services,
                IConfiguration configuration)
            => services
                .AddServices(typeof(BotSetup).Assembly, configuration);
    }
}
