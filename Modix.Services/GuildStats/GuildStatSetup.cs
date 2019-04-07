using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

namespace Modix.Services.GuildStats
{
    public static class GuildStatSetup
    {
        public static IServiceCollection AddGuildStats(this IServiceCollection services)
            => services
                .AddScoped<IGuildStatService, GuildStatService>()
                .AddScoped<INotificationHandler<UserJoinedNotification>, GuildStatService>()
                .AddScoped<INotificationHandler<UserLeftNotification>, GuildStatService>();
    }
}
