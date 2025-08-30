using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using Moq;
using Shouldly;

namespace Modix.Common.Test.Extensions.Microsoft.Extensions.DependencyInjection
{
    [TestFixture]
    public class ServiceCollectionBindingExtensionsTests
    {
        #region Fakes

        private static Assembly MakeFakeAssembly(
            params TypeInfo[] definedTypes)
        {
            var mockAssembly = new Mock<Assembly>();

            mockAssembly
                .Setup(x => x.DefinedTypes)
                .Returns(definedTypes);

            return mockAssembly.Object;
        }

        private static IConfiguration MakeFakeConfiguration(
            params KeyValuePair<string, string>[] configuredValues)
        {
            var mockConfiguration = new Mock<IConfiguration>();

            foreach (var configuredValue in configuredValues)
                mockConfiguration
                    .Setup(x => x[configuredValue.Key])
                    .Returns(configuredValue.Value);

            return mockConfiguration.Object;
        }

        private class FakeType { }

        [ServiceBinding(ServiceLifetime.Singleton)]
        private class FakeSimpleService1 { }

        [ServiceBinding(ServiceLifetime.Scoped)]
        private class FakeSimpleService2 { }

        [ServiceBinding(ServiceLifetime.Transient)]
        private class FakeSimpleService3 { }

        private abstract class FakeConfiguredServiceBase
        {
            protected FakeConfiguredServiceBase(
                string configuredValue)
            {
                ConfiguredValue = configuredValue;
            }

            public string ConfiguredValue { get; }
        }

        private class FakeConfiguredService1
            : FakeConfiguredServiceBase
        {
            public FakeConfiguredService1(
                    string configuredValue)
                : base(
                    configuredValue) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator1
            : IServiceConfigurator
        {
            public void ConfigureServices(
                    IServiceCollection services,
                    IConfiguration configuration)
                => services.AddSingleton(new FakeConfiguredService1(
                    configuration[nameof(FakeConfiguredService1)]!));
        }

        private class FakeConfiguredService2
            : FakeConfiguredServiceBase
        {
            public FakeConfiguredService2(
                    string configuredValue)
                : base(
                    configuredValue) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator2
            : IServiceConfigurator
        {
            public void ConfigureServices(
                    IServiceCollection services,
                    IConfiguration configuration)
                => services.AddSingleton(new FakeConfiguredService2(
                    configuration[nameof(FakeConfiguredService2)]!));
        }

        private class FakeConfiguredService3
            : FakeConfiguredServiceBase
        {
            public FakeConfiguredService3(
                    string configuredValue)
                : base(
                    configuredValue) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator3
            : IServiceConfigurator
        {
            public void ConfigureServices(
                    IServiceCollection services,
                    IConfiguration configuration)
                => services.AddSingleton(new FakeConfiguredService3(
                    configuration[nameof(FakeConfiguredService3)]!));
        }

        #endregion Fakes

        #region AddServices() Tests

        public class ServiceDescriptorExpectation
        {
            public ServiceDescriptorExpectation(
                Type serviceType,
                Type? implementationType,
                string? implementationInstanceConfiguredValue,
                ServiceLifetime lifetime)
            {
                ImplementationInstanceConfiguredValue = implementationInstanceConfiguredValue;
                ImplementationType = implementationType;
                Lifetime = lifetime;
                ServiceType = serviceType;
            }

            public string? ImplementationInstanceConfiguredValue { get; }

            public Type? ImplementationType { get; }

            public ServiceLifetime Lifetime { get; }

            public Type ServiceType { get; }

            public bool MatchesActual(
                    ServiceDescriptor actual)
                => (actual.ImplementationType == ImplementationType)
                    && (actual.Lifetime == Lifetime)
                    && (actual.ServiceType == ServiceType)
                    && (ImplementationInstanceConfiguredValue is null)
                        ? (actual.ImplementationInstance is null)
                        : ((actual.ImplementationInstance is FakeConfiguredServiceBase instance)
                            && (instance.ConfiguredValue == ImplementationInstanceConfiguredValue));
        }

        public static readonly ImmutableArray<TestCaseData> AddServices_TestCaseData
            = ImmutableArray.Create(
                    new TestCaseData(
                            MakeFakeAssembly(),
                            MakeFakeConfiguration(),
                            ImmutableArray<ServiceDescriptorExpectation>.Empty)
                        .SetName("{m}(Assembly is empty)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeType).GetTypeInfo()),
                            MakeFakeConfiguration(),
                            ImmutableArray<ServiceDescriptorExpectation>.Empty)
                        .SetName("{m}(Assembly contains no relevant types)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSimpleService1).GetTypeInfo(),
                                typeof(FakeSimpleService2).GetTypeInfo(),
                                typeof(FakeSimpleService3).GetTypeInfo()),
                            MakeFakeConfiguration(),
                            ImmutableArray.Create(
                                /*                                  serviceType,                implementationType,         implementationInstanceConfiguredValue,  lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService1), typeof(FakeSimpleService1), null,                                   ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService2), typeof(FakeSimpleService2), null,                                   ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService3), typeof(FakeSimpleService3), null,                                   ServiceLifetime.Transient   )))
                        .SetName("{m}(Assembly contains ServiceBindings)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeConfiguredServiceBase).GetTypeInfo(),
                                typeof(FakeConfiguredService1).GetTypeInfo(),
                                typeof(FakeConfiguredService2).GetTypeInfo(),
                                typeof(FakeConfiguredService3).GetTypeInfo(),
                                typeof(FakeServiceConfigurator1).GetTypeInfo(),
                                typeof(FakeServiceConfigurator2).GetTypeInfo(),
                                typeof(FakeServiceConfigurator3).GetTypeInfo()),
                            MakeFakeConfiguration(
                                /*                                  key,                            value               */
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService1), "ConfiguredValue1"  ),
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService2), "ConfiguredValue2"  ),
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService3), "ConfiguredValue3"  )),
                            ImmutableArray.Create(
                                /*                                  serviceType,                    implementationType, implementationInstanceConfiguredValue,  lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService1), null,               "ConfiguredValue1",                     ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService2), null,               "ConfiguredValue2",                     ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService3), null,               "ConfiguredValue3",                     ServiceLifetime.Singleton   )))
                        .SetName("{m}(Assembly contains ServiceConfigurator)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSimpleService1).GetTypeInfo(),
                                typeof(FakeSimpleService2).GetTypeInfo(),
                                typeof(FakeSimpleService3).GetTypeInfo(),
                                typeof(FakeConfiguredServiceBase).GetTypeInfo(),
                                typeof(FakeConfiguredService1).GetTypeInfo(),
                                typeof(FakeConfiguredService2).GetTypeInfo(),
                                typeof(FakeConfiguredService3).GetTypeInfo(),
                                typeof(FakeServiceConfigurator1).GetTypeInfo(),
                                typeof(FakeServiceConfigurator2).GetTypeInfo(),
                                typeof(FakeServiceConfigurator3).GetTypeInfo()),
                            MakeFakeConfiguration(
                                /*                                  key,                            value               */
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService1), "ConfiguredValue1"  ),
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService2), "ConfiguredValue2"  ),
                                new KeyValuePair<string, string>(   nameof(FakeConfiguredService3), "ConfiguredValue3"  )),
                            ImmutableArray.Create(
                                /*                                  serviceType,                    implementationType,         implementationInstanceConfiguredValue,  lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService1),     typeof(FakeSimpleService1), null,                                   ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService2),     typeof(FakeSimpleService2), null,                                   ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleService3),     typeof(FakeSimpleService3), null,                                   ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService1), null,                       "ConfiguredValue1",                     ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService2), null,                       "ConfiguredValue2",                     ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeConfiguredService3), null,                       "ConfiguredValue3",                     ServiceLifetime.Singleton   )))
                        .SetName("{m}(Assembly contains various relevant types)")
                );

        [TestCaseSource(nameof(AddServices_TestCaseData))]
        public void AddServices_Always_ResultsAreExpected(
            Assembly assembly,
            IConfiguration configuration,
            ImmutableArray<ServiceDescriptorExpectation> expectedResults)
        {
            var serviceDescriptors = new ServiceCollection();

            serviceDescriptors.AddServices(assembly, configuration);

            var unmatchedResults = serviceDescriptors.ToHashSet();

            foreach (var expectedResult in expectedResults)
            {
                serviceDescriptors.ShouldContain(result => expectedResult.MatchesActual(result));
                unmatchedResults.RemoveWhere(result => expectedResult.MatchesActual(result));
            }

            unmatchedResults.ShouldBeEmpty();
        }

        #endregion EnumerateServiceDescriptors() Tests
    }
}
