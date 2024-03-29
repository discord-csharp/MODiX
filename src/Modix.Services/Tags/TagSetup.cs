using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;
using Modix.Data.Repositories;

namespace Modix.Services.Tags
{
    /// <summary>
    /// Contains extension methods for configuring the tags feature upon application startup.
    /// </summary>
    public static class TagSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the tags feature to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the tag services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixTags(this IServiceCollection services)
            => services
                .AddScoped<ITagService, TagService>()
                .AddScoped<INotificationHandler<MessageReceivedNotification>, TagInlineParsingHandler>();
    }
}
