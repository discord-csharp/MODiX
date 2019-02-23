using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Messages.Discord;

namespace Modix.Services.Starboard
{
    public static class StarboardSetup
    {
        public static IServiceCollection AddStarboard(this IServiceCollection services)
            => services
                .AddScoped<StarboardService>()
                .AddScoped<INotificationHandler<ReactionAdded>, StarboardHandler>()
                .AddScoped<INotificationHandler<ReactionRemoved>, StarboardHandler>();
    }
}
