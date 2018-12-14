using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Messages.Discord;

namespace Modix.Services.GuildStats
{
    public static class GuildStatSetup
    {
        public static IServiceCollection AddGuildStats(this IServiceCollection services)
            => services
                .AddScoped<IGuildStatService, GuildStatService>()
                .AddScoped<INotificationHandler<UserJoined>, GuildStatService>()
                .AddScoped<INotificationHandler<UserLeft>, GuildStatService>();
    }
}
