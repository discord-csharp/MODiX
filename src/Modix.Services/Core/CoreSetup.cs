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
                .AddSingleton<IBehavior, DiscordSocketListeningBehavior>()
                .AddScoped<IAuthorizationService, AuthorizationService>()
                .AddScoped<IChannelService, ChannelService>()
                .AddScoped<ChannelTrackingBehavior>()
                .AddScoped<INotificationHandler<ChannelCreatedNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<ChannelUpdatedNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<GuildAvailableNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<INotificationHandler<JoinedGuildNotification>>(x => x.GetService<ChannelTrackingBehavior>())
                .AddScoped<IUserService, UserService>()
                .AddScoped<IGuildChannelRepository, GuildChannelRepository>()
                .AddScoped<IGuildRoleRepository, GuildRoleRepository>()
                .AddScoped<IGuildUserRepository, GuildUserRepository>()
                .AddScoped<DesignatedChannelService>()
                .AddScoped<IDesignatedRoleService, DesignatedRoleService>()
                .AddScoped<IClaimMappingRepository, ClaimMappingRepository>()
                .AddScoped<IConfigurationActionRepository, ConfigurationActionRepository>()
                .AddScoped<IDesignatedRoleMappingRepository, DesignatedRoleMappingRepository>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .AddScoped<IMessageService, MessageService>();
    }
}
