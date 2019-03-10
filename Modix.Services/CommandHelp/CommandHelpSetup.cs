using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Messages.Discord;

namespace Modix.Services.CommandHelp
{
    /// <summary>
    /// Contains extension methods for configuration the CommandHelp feature
    /// </summary>
    public static class CommandHelpSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the Command Help feature, to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Command Help services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddCommandHelp(this IServiceCollection services)
            => services
                .AddSingleton<ICommandHelpService, CommandHelpService>()
                .AddSingleton<CommandErrorHandler>()
                .AddSingleton<INotificationHandler<ReactionAdded>>(x => x.GetService<CommandErrorHandler>())
                .AddSingleton<INotificationHandler<ReactionRemoved>>(x => x.GetService<CommandErrorHandler>());
    }
}
