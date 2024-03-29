using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionBindingExtensions
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services,
            Assembly assembly,
            IConfiguration configuration)
        {
            foreach (var serviceDescriptor in ServiceBindingAttribute.EnumerateServiceDescriptors(assembly))
                services.Add(serviceDescriptor);

            foreach (var serviceConfigurator in ServiceConfiguratorAttribute.EnumerateServiceConfigurators(assembly))
                serviceConfigurator.ConfigureServices(services, configuration);

            return services;
        }
    }
}
