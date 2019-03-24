using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Repositories;
using Modix.Services.Messages.Discord;

namespace Modix.Services.EmojiStats
{
    /// <summary>
    /// Contains extension methods for configuring the emoji stats feature upon application startup.
    /// </summary>
    public static class EmojiStatsSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the emoji stats feature to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the emoji stats services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddEmojiStats(this IServiceCollection services)
            => services
                .AddScoped<IEmojiRepository, EmojiRepository>()
                .AddScoped<INotificationHandler<ReactionAdded>, EmojiUsageHandler>()
                .AddScoped<INotificationHandler<ReactionRemoved>, EmojiUsageHandler>()
                .AddScoped<INotificationHandler<ChatMessageReceived>, EmojiUsageHandler>()
                .AddScoped<INotificationHandler<ChatMessageUpdated>, EmojiUsageHandler>()
                .AddScoped<INotificationHandler<ChatMessageDeleted>, EmojiUsageHandler>()
                .AddScoped<IEmojiStatsService, EmojiStatsService>();
    }
}
