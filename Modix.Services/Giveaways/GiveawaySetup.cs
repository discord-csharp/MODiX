using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Giveaways
{
    /// <summary>
    /// Contains extension methods for configuring the giveaways feature upon application startup.
    /// </summary>
    public static class GiveawaySetup
    {
        /// <summary>
        /// Adds the services and classes that make up the giveaways feature to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the giveaways services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddGiveaways(this IServiceCollection services)
            => services
                .AddScoped<IGiveawayService, GiveawayService>();
    }
}
