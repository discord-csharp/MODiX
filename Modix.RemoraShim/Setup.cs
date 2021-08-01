using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Modix.Data.Models.Core;
using Modix.RemoraShim.Behaviors;
using Modix.RemoraShim.Commands;
using Modix.RemoraShim.Services;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;

namespace Modix.RemoraShim
{
    public static class Setup
    {
        public static IServiceCollection AddRemoraShim(this IServiceCollection services, IConfiguration configuration)
            => services
                .AddServices(typeof(Setup).Assembly, configuration)
                .AddTransient<IThreadService, ThreadService>()
                .AddDiscordGateway(x => x.GetRequiredService<IOptions<ModixConfig>>().Value.DiscordToken)
                .Configure<DiscordGatewayClientOptions>(x => x.Intents =
                    GatewayIntents.GuildBans |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.Guilds)
                .AddHostedService<ModixBot>() 
                .AddCommands()
                .AddResponders();
    }
}
