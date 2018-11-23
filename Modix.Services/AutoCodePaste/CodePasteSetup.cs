using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Messages.Discord;

namespace Modix.Services.AutoCodePaste
{
    public static class AutoCodePasteSetup
    {
        public static IServiceCollection AddAutoCodePaste(this IServiceCollection services)
            => services
                .AddScoped<INotificationHandler<ReactionAdded>, CodePasteHandler>()
                .AddScoped<INotificationHandler<ReactionRemoved>, CodePasteHandler>();
    }
}
