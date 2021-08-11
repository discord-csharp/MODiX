using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

using Remora.Discord.Gateway.Extensions;

namespace Modix.RemoraShim.Responders
{
    internal static class Setup
    {
        public static IServiceCollection AddResponders(this IServiceCollection services)
            => services
                .AddResponder<TrackingResponder>()
                .AddResponder<MuteRoleConfigurationResponder>()
                .AddScoped<INotificationHandler<Discord.ReadyNotification>, MuteRoleConfigurationResponder>()
                .AddResponder<MessageLoggingResponder>()
                .AddResponder<MessageCheckResponder>()
                .AddResponder<InlineTagResponder>();

    }
}
