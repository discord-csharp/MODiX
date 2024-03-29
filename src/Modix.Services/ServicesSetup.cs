#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services
{
    public static class ServicesSetup
    {
        public static IServiceCollection AddModixServices(
                this IServiceCollection services,
                IConfiguration configuration)
            => services
                .AddServices(typeof(ServicesSetup).Assembly, configuration);
    }
}
