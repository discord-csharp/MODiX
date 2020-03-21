using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceBindingAttribute
        : Attribute
    {
        public static IEnumerable<ServiceDescriptor> EnumerateServiceDescriptors(
                Assembly assembly)
            => assembly.DefinedTypes
                .Select(typeInfo => (typeInfo, attribute: typeInfo.GetCustomAttribute<ServiceBindingAttribute>()))
                .Where(mapping => mapping.attribute is { })
                .SelectMany(mapping =>
                {
                    var attribute = mapping.attribute!;
                    var implementationType = mapping.typeInfo.AsType();
                    var interfaces = mapping.typeInfo.ImplementedInterfaces
                        .Where(x => x != typeof(IDisposable))
                        .ToArray();

                    return interfaces switch
                    {
                        { Length: 0 } => Enumerable.Empty<ServiceDescriptor>()
                            .Append(ServiceDescriptor.Describe(
                                serviceType:        implementationType,
                                implementationType: implementationType,
                                lifetime:           attribute.Lifetime)),
                        
                        { Length: 1 } => Enumerable.Empty<ServiceDescriptor>()
                            .Append(ServiceDescriptor.Describe(
                                serviceType:        interfaces[0],
                                implementationType: implementationType,
                                lifetime:           attribute.Lifetime)),
                        
                        _ => interfaces
                            .Select(@interface => ServiceDescriptor.Describe(
                                serviceType:            @interface,
                                implementationFactory:  serviceProvider => serviceProvider.GetRequiredService(implementationType),
                                lifetime:               attribute.Lifetime))
                            .Append(ServiceDescriptor.Describe(
                                serviceType:        implementationType,
                                implementationType: implementationType,
                                lifetime:           attribute.Lifetime)),
                    };
                });

        public ServiceBindingAttribute(
            ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ServiceLifetime Lifetime { get; }
    }
}
