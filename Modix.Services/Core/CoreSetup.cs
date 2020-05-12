using Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Modix.Common.Messaging;
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
                .AddSingleton<IBehavior, RoleTrackingBehavior>()
                .AddSingleton<IBehavior, UserTrackingBehavior>()
                .AddSingleton<IBehavior, MessageLogBehavior>()
                .AddSingleton<IBehavior, StatsBehavior>()
                .AddSingleton<IBehavior, DiscordSocketListeningBehavior>()
                .AddSingleton<ReadySynchronizationProvider>()
                .AddSingleton<IReadySynchronizationProvider>(x => x.GetService<ReadySynchronizationProvider>())
                .AddSingleton<INotificationHandler<ReadyNotification>>(x => x.GetService<ReadySynchronizationProvider>())
                .AddSingleton<ISelfUserProvider, SelfUserProvider>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<AuthorizationAutoConfigBehavior>()
                .AddScoped<INotificationHandler<GuildAvailableNotification>>(x => x.GetService<AuthorizationAutoConfigBehavior>())
                .AddScoped<INotificationHandler<JoinedGuildNotification>>(x => x.GetService<AuthorizationAutoConfigBehavior>())
                .AddScoped<IChannelService, ChannelService>()
                .AddScoped<ChannelTrackingBehavior>()
                .AddScoped<INotificationHandler<ChannelCreatedNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<ChannelUpdatedNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<GuildAvailableNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<JoinedGuildNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped<IGuildChannelRepository, GuildChannelRepository>()
                .AddScoped<IGuildRoleRepository, GuildRoleRepository>()
                .AddScoped<IGuildUserRepository, GuildUserRepository>()
                .AddScoped<IDesignatedChannelService, DesignatedChannelService>()
                .AddScoped<IDesignatedRoleService, DesignatedRoleService>()
                .AddScoped<IClaimMappingRepository, ClaimMappingRepository>()
                .AddScoped<IConfigurationActionRepository, ConfigurationActionRepository>()
                .AddScoped<IDesignatedChannelMappingRepository, DesignatedChannelMappingRepository>()
                .AddScoped<IDesignatedRoleMappingRepository, DesignatedRoleMappingRepository>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .AddScoped<IMessageService, MessageService>();
    }
}
