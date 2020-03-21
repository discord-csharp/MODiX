using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Common
{
    public static class CommonSetup
    {
        public static IServiceCollection AddModixCommon(
                this IServiceCollection services,
                IConfiguration configuration)
            => services
                .AddServices(typeof(CommonSetup).Assembly, configuration);
    }
}
