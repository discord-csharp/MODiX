using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using Moq;
using Shouldly;

namespace Modix.Common.Test.Extensions.Microsoft.Extensions.DependencyInjection
{
    [TestFixture]
    public class ServiceBindingAttributeTests
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

        private class FakeType { }

        [ServiceBinding(ServiceLifetime.Singleton)]
        private class FakeSimpleSingletonService { }

        [ServiceBinding(ServiceLifetime.Scoped)]
        private class FakeSimpleScopedService { }

        [ServiceBinding(ServiceLifetime.Transient)]
        private class FakeSimpleTransientService { }

        private interface IFakeAbstractService1 { }
        
        private interface IFakeAbstractService2 { }
        
        private interface IFakeAbstractService3 { }

        [ServiceBinding(ServiceLifetime.Singleton)]
        private class FakeAbstractService
            : IFakeAbstractService1 { }

        [ServiceBinding(ServiceLifetime.Scoped)]
        private class FakeSharedAbstractService
            : IFakeAbstractService1,
                IFakeAbstractService2,
                IFakeAbstractService3 { }

        [ServiceBinding(ServiceLifetime.Transient)]
        private sealed class FakeDisposableService
            : IFakeAbstractService1,
                IFakeAbstractService2,
                IFakeAbstractService3,
                IDisposable
        {
            public void Dispose() { }
        }

        #endregion Fakes

        #region Constructor Tests

        [TestCase(ServiceLifetime.Scoped)]
        [TestCase(ServiceLifetime.Singleton)]
        [TestCase(ServiceLifetime.Transient)]
        public void Constructor_Always_InitialzesInstance(
            ServiceLifetime lifetime)
        {
            var result = new ServiceBindingAttribute(
                lifetime);

            result.Lifetime.ShouldBe(lifetime);
        }

        #endregion Constructor Tests

        #region EnumerateServiceDescriptors() Tests

        public class ServiceDescriptorExpectation
        {
            public ServiceDescriptorExpectation(
                Type serviceType,
                Type? implementationType,
                Type? implementationFactoryType,
                ServiceLifetime lifetime)
            {
                ImplementationFactoryServiceType = implementationFactoryType;
                ImplementationType = implementationType;
                Lifetime = lifetime;
                ServiceType = serviceType;
            }

            public Type? ImplementationFactoryServiceType { get; }

            public Type? ImplementationType { get; }

            public ServiceLifetime Lifetime { get; }

            public Type ServiceType { get; }

            public bool MatchesActual(
                    ServiceDescriptor actual)
                => ((actual.ImplementationFactory is null) == (ImplementationFactoryServiceType is null))
                    && (actual.ImplementationType == ImplementationType)
                    && (actual.Lifetime == Lifetime)
                    && (actual.ServiceType == ServiceType);
        }

        public static readonly ImmutableArray<TestCaseData> EnumerateServiceDescriptors_TestCaseData
            = ImmutableArray.Create(
                    new TestCaseData(
                            MakeFakeAssembly(),
                            ImmutableArray<ServiceDescriptorExpectation>.Empty)
                        .SetName("{m}(Empty Assembly)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeType).GetTypeInfo()),
                            ImmutableArray<ServiceDescriptorExpectation>.Empty)
                        .SetName("{m}(Type has no ServiceBinding)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSimpleSingletonService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                        implementationType,                 implementationFactoryServiceType,   lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleSingletonService), typeof(FakeSimpleSingletonService), null,                               ServiceLifetime.Singleton   )))
                        .SetName("{m}(ServiceBinding.Lifetime is Singleton)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSimpleScopedService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                        implementationType,                 implementationFactoryServiceType,   lifetime                */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleScopedService),    typeof(FakeSimpleScopedService),    null,                               ServiceLifetime.Scoped  )))
                        .SetName("{m}(ServiceBinding.Lifetime is Scoped)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSimpleTransientService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                        implementationType,                 implementationFactoryServiceType,   lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleTransientService), typeof(FakeSimpleTransientService), null,                               ServiceLifetime.Transient   )))
                        .SetName("{m}(ServiceBinding.Lifetime is Transient)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeAbstractService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                   implementationType,          implementationFactoryServiceType,   lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1), typeof(FakeAbstractService), null,                               ServiceLifetime.Singleton   )))
                        .SetName("{m}(Service implements an interface)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeSharedAbstractService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                        implementationType,                 implementationFactoryServiceType,   lifetime                */
                                new ServiceDescriptorExpectation(   typeof(FakeSharedAbstractService),  typeof(FakeSharedAbstractService),  null,                               ServiceLifetime.Scoped  ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped  ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService2),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped  ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService3),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped  )
                            ))
                        .SetName("{m}(Service implements many interfaces)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeDisposableService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                    implementationType,             implementationFactoryServiceType,   lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeDisposableService),  typeof(FakeDisposableService),  null,                               ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1),  null,                           typeof(FakeDisposableService),      ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService2),  null,                           typeof(FakeDisposableService),      ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService3),  null,                           typeof(FakeDisposableService),      ServiceLifetime.Transient   )
                            ))
                        .SetName("{m}(Service is Disposable)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeType).GetTypeInfo(),
                                typeof(FakeSimpleSingletonService).GetTypeInfo(),
                                typeof(FakeSimpleScopedService).GetTypeInfo(),
                                typeof(FakeSimpleTransientService).GetTypeInfo(),
                                typeof(FakeAbstractService).GetTypeInfo(),
                                typeof(FakeSharedAbstractService).GetTypeInfo(),
                                typeof(FakeDisposableService).GetTypeInfo()),
                            ImmutableArray.Create(
                                /*                                  serviceType,                        implementationType,                 implementationFactoryServiceType,   lifetime                    */
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleSingletonService), typeof(FakeSimpleSingletonService), null,                               ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleScopedService),    typeof(FakeSimpleScopedService),    null,                               ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(FakeSimpleTransientService), typeof(FakeSimpleTransientService), null,                               ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1),      typeof(FakeAbstractService),        null,                               ServiceLifetime.Singleton   ),
                                new ServiceDescriptorExpectation(   typeof(FakeSharedAbstractService),  typeof(FakeSharedAbstractService),  null,                               ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService2),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService3),      null,                               typeof(FakeSharedAbstractService),  ServiceLifetime.Scoped      ),
                                new ServiceDescriptorExpectation(   typeof(FakeDisposableService),      typeof(FakeDisposableService),      null,                               ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService1),      null,                               typeof(FakeDisposableService),      ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService2),      null,                               typeof(FakeDisposableService),      ServiceLifetime.Transient   ),
                                new ServiceDescriptorExpectation(   typeof(IFakeAbstractService3),      null,                               typeof(FakeDisposableService),      ServiceLifetime.Transient   )
                            ))
                        .SetName("{m}(Many ServiceBindings present)")
                );

        [TestCaseSource(nameof(EnumerateServiceDescriptors_TestCaseData))]
        public void EnumerateServiceDescriptors_Always_ResultsAreExpected(
            Assembly assembly,
            ImmutableArray<ServiceDescriptorExpectation> expectedResults)
        {
            var results = ServiceBindingAttribute.EnumerateServiceDescriptors(
                    assembly)
                .ToArray();

            var unmatchedResults = results.ToHashSet();

            foreach(var expectedResult in expectedResults)
            {
                results.ShouldContain(result => expectedResult.MatchesActual(result));

                var result = unmatchedResults.First(result => expectedResult.MatchesActual(result));

                if(expectedResult.ImplementationFactoryServiceType is { })
                {
                    var mockServiceProvider = new Mock<IServiceProvider>();
                    var service = new object();

                    mockServiceProvider
                        .Setup(x => x.GetService(expectedResult.ImplementationFactoryServiceType))
                        .Returns(service);

                    result.ImplementationFactory?.Invoke(mockServiceProvider.Object)
                        .ShouldBeSameAs(service);
                }

                unmatchedResults.Remove(result);
            }

            unmatchedResults.ShouldBeEmpty();
        }

        #endregion EnumerateServiceDescriptors() Tests
    }
}
