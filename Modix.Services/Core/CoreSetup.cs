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
                .AddSingleton<IBehavior, AuthorizationAutoConfigBehavior>()
                .AddSingleton<IBehavior, UserTrackingBehavior>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IChannelService, ChannelService>()
                .AddScoped<IGuildUserRepository, GuildUserRepository>()
                .AddScoped<IGuildChannelRepository, GuildChannelRepository>()
                .AddScoped<IClaimMappingRepository, ClaimMappingRepository>()
                .AddScoped<IConfigurationActionRepository, ConfigurationActionRepository>();
    }
}
