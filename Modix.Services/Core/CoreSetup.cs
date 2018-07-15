using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    public static class CoreSetup
    {
        public static IServiceCollection AddModixCore(this IServiceCollection services)
            => services
                .AddSingleton<IBehavior, UserMonitorBehavior>()
                .AddScoped<IGuildService, GuildService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IUserRepository, UserRepository>();
    }
}
