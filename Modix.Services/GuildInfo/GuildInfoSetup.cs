using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Messages.Discord;

namespace Modix.Services.GuildInfo
{
    public static class GuildInfoSetup
    {
        public static IServiceCollection AddGuildInfo(this IServiceCollection services)
            => services
                .AddSingleton<GuildInfoService>()
                // resolve the singleton from within the scope
                .AddScoped<INotificationHandler<UserJoined>, GuildInfoService>(p => p.GetService<GuildInfoService>())
                .AddScoped<INotificationHandler<UserLeft>, GuildInfoService>(p => p.GetService<GuildInfoService>());
    }
}
