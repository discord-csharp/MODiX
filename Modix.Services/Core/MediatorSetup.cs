using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Core
{
    public static class MediatorSetup
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
            => services
                .AddScoped<IMediator, Mediator>()
                .AddScoped<ServiceFactory>(p => p.GetService);
    }
}
