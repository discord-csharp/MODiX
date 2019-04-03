using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Messages.Discord;

namespace Modix.Services.GuildStats
{
    public static class GuildStatSetup
    {
        public static IServiceCollection AddGuildStats(this IServiceCollection services)
            => services
                .AddScoped<IGuildStatService, GuildStatService>()
                .AddScoped<Common.Messaging.INotificationHandler<UserJoinedNotification>, GuildStatService>()
                .AddScoped<MediatR.INotificationHandler<UserLeft>, GuildStatService>();
    }
}
