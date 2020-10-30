﻿using Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Modix.Common.Messaging;
using Modix.Data.Repositories;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Contains extension methods for configuring the Moderation feature, upon application startup.
    /// </summary>
    public static class ModerationSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Moderation feature, to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Moderation services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddModixModeration(this IServiceCollection services)
            => services
                .AddSingleton<IBehavior, ModerationAutoConfigBehavior>()
                .AddSingleton<ModerationAutoRescindBehavior>()
                .AddSingleton<IBehavior>(serviceProvider =>
                    serviceProvider.GetRequiredService<ModerationAutoRescindBehavior>())
                .AddSingleton<IInfractionEventHandler>(serviceProvider =>
                    serviceProvider.GetRequiredService<ModerationAutoRescindBehavior>())
                .AddScoped<IModerationService, ModerationService>()
                .AddScoped<IModerationActionRepository, ModerationActionRepository>()
                .AddScoped<IInfractionRepository, InfractionRepository>()
                .AddScoped<IDeletedMessageRepository, DeletedMessageRepository>()
                .AddScoped<IDeletedMessageBatchRepository, DeletedMessageBatchRepository>()
                .AddScoped<INotificationHandler<MessageReceivedNotification>, MessageContentCheckBehaviour>()
                .AddScoped<INotificationHandler<MessageUpdatedNotification>, MessageContentCheckBehaviour>()
                .AddScoped<INotificationHandler<UserJoinedNotification>, MutePersistingHandler>()
                .AddScoped<INotificationHandler<UserBannedNotification>, InfractionSyncingHandler>();
    }
}
