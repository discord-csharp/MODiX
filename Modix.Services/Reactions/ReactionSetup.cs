using MediatR;
using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Repositories;
using Modix.Services.Messages.Discord;

namespace Modix.Services.Reactions
{
    /// <summary>
    /// Contains extension methods for configuring the reactions feature upon application startup.
    /// </summary>
    public static class ReactionSetup
    {
        /// <summary>
        /// Adds the services and classes that make up the reactions feature to a service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the reaction services are to be added.</param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddReactions(this IServiceCollection services)
            => services
                .AddScoped<IReactionRepository, ReactionRepository>()
                .AddScoped<INotificationHandler<ReactionAdded>, ReactionHandler>()
                .AddScoped<INotificationHandler<ReactionRemoved>, ReactionHandler>();
    }
}
