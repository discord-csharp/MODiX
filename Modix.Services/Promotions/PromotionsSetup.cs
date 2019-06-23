using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;
using Modix.Data.Repositories;

namespace Modix.Services.Promotions
{
    /// <summary>
    /// Contains extension methods for configuring the Promotions feature, upon application startup.
    /// </summary>
    public static class PromotionsSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Promotions feature, to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Moderation services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixPromotions(this IServiceCollection services)
            => services
                .AddScoped<IPromotionsService, PromotionsService>()
                .AddScoped<IPromotionActionRepository, PromotionActionRepository>()
                .AddScoped<IPromotionCampaignRepository, PromotionCampaignRepository>()
                .AddScoped<IPromotionCommentRepository, PromotionCommentRepository>();
    }
}
