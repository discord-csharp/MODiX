using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceConfigurator
    {
        void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration);
    }
}
