using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Repositories;

namespace Modix.Services.Mentions
{
    /// <summary>
    /// Contains extension methods for configuring the Mention feature upon application startup.
    /// </summary>
    public static class MentionSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Mention feature to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Mention services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixMentions(this IServiceCollection services)
            => services
                .AddSingleton<IBehavior, MentionAutoConfigBehavior>()
                .AddScoped<IMentionMappingRepository, MentionMappingRepository>()
                .AddScoped<IMentionService, MentionService>();
    }
}
