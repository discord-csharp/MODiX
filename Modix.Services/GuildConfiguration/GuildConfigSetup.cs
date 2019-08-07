using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Repositories;

namespace Modix.Services.GuildConfiguration
{
    /// <summary>
    /// Contains extension methods for configuring the Guild Configuration feature, upon application startup.
    /// </summary>
    public static class GuildConfigSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Guild Configuration feature, to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Moderation services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixPromotions(this IServiceCollection services)
            => services
                .AddScoped<IGuildConfigurationRepository, GuildConfigurationRepository>();
    }
}
