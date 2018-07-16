using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Contains extension methods for configuring the Modix Core services and features, upon application startup.
    /// </summary>
    public static class CoreSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Modix Core, to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Core services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixCore(this IServiceCollection services)
            => services
                .AddScoped<IAuthenticationService, AuthenticationService>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddSingleton<IBehavior, UserMonitorBehavior>()
                .AddScoped<IGuildService, GuildService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IUserRepository, UserRepository>();
    }
}
