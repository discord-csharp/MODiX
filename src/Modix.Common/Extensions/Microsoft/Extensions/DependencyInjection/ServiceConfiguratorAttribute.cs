using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceConfiguratorAttribute
        : Attribute
    {
        public static IEnumerable<IServiceConfigurator> EnumerateServiceConfigurators(
                Assembly assembly)
            => assembly.DefinedTypes
                .Where(typeInfo => typeInfo.GetCustomAttribute<ServiceConfiguratorAttribute>() is { })
                .Select(typeInfo =>
                {
                    if (!typeof(IServiceConfigurator).IsAssignableFrom(typeInfo))
                        throw new InvalidOperationException($"Invalid use of {typeof(ServiceConfiguratorAttribute)} upon type {typeInfo}: Type must implement {typeof(IServiceConfigurator)}.");

                    var constructor = typeInfo.GetConstructor(Type.EmptyTypes);
                    if(constructor is null)
                        throw new InvalidOperationException($"Invalid use of {typeof(ServiceConfiguratorAttribute)} upon type {typeInfo}: Type must have a public parameterless constructor.");

                    return (IServiceConfigurator)constructor.Invoke(null);
                });
    }
}
