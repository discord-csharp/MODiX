using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

namespace Modix.Services.Starboard
{
    public static class StarboardSetup
    {
        public static IServiceCollection AddStarboard(this IServiceCollection services)
            => services
                .AddScoped<IStarboardService, StarboardService>();
    }
}
