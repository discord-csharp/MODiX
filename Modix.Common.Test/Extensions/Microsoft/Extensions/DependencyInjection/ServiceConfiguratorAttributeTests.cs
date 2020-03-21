using System;
using System.Collections.Immutable;
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
    public class ServiceConfiguratorAttributeTests
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

        [ServiceConfigurator]
        private class InvalidFakeServiceConfigurator1 { }

        [ServiceConfigurator]
        private class InvalidFakeServiceConfigurator2
            : IServiceConfigurator
        {
            private InvalidFakeServiceConfigurator2() { }

            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        [ServiceConfigurator]
        private class InvalidFakeServiceConfigurator3
            : IServiceConfigurator
        {
            public InvalidFakeServiceConfigurator3(
                #pragma warning disable IDE0060 // Remove unused parameter
                string value) { }
                #pragma warning restore IDE0060 // Remove unused parameter

            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        [ServiceConfigurator]
        private class UnattachedFakeServiceConfigurator
            : IServiceConfigurator
        {
            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator1
            : IServiceConfigurator
        {
            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator2
            : IServiceConfigurator
        {
            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        [ServiceConfigurator]
        private class FakeServiceConfigurator3
            : IServiceConfigurator
        {
            public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration) { }
        }

        #endregion Fakes

        #region EnumerateServiceConfigurators() Tests

        public static readonly ImmutableArray<TestCaseData> EnumerateServiceConfigurators_AssemblyIsNotValid_TestCaseData
            = ImmutableArray.Create(
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(InvalidFakeServiceConfigurator1).GetTypeInfo()),
                            typeof(InvalidFakeServiceConfigurator1))
                        .SetName("{m}(ServiceConfigurator does not implement IServiceConfigurator)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(InvalidFakeServiceConfigurator2).GetTypeInfo()),
                            typeof(InvalidFakeServiceConfigurator2))
                        .SetName("{m}(ServiceConfigurator constructor is not public)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(InvalidFakeServiceConfigurator3).GetTypeInfo()),
                            typeof(InvalidFakeServiceConfigurator3))
                        .SetName("{m}(ServiceConfigurator constructor is not parameterless)"));

        [TestCaseSource(nameof(EnumerateServiceConfigurators_AssemblyIsNotValid_TestCaseData))]
        public void EnumerateServiceConfigurators_AssemblyIsNotValid_ThrowsException(
            Assembly assembly,
            Type invalidConfiguratorType)
        {
            var result = Should.Throw<InvalidOperationException>(() =>
            {
                _ = ServiceConfiguratorAttribute.EnumerateServiceConfigurators(
                        assembly)
                    .ToArray();
            });

            result.Message.ShouldContain(invalidConfiguratorType.ToString());
        }

        public static readonly ImmutableArray<TestCaseData> EnumerateServiceConfigurators_AssemblyIsValid_TestCaseData
            = ImmutableArray.Create(
                    new TestCaseData(
                            MakeFakeAssembly(),
                            ImmutableArray<Type>.Empty)
                        .SetName("{m}(Assembly is empty)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeType).GetTypeInfo()),
                            ImmutableArray<Type>.Empty)
                        .SetName("{m}(Assembly contains no ServiceConfigurators)"),
                    
                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeServiceConfigurator1).GetTypeInfo()),
                            ImmutableArray.Create(
                                typeof(FakeServiceConfigurator1)))
                        .SetName("{m}(Assembly contains a ServiceConfigurator)"),

                    new TestCaseData(
                            MakeFakeAssembly(
                                typeof(FakeServiceConfigurator1).GetTypeInfo(),
                                typeof(FakeServiceConfigurator2).GetTypeInfo(),
                                typeof(FakeServiceConfigurator3).GetTypeInfo()),
                            ImmutableArray.Create(
                                typeof(FakeServiceConfigurator1),
                                typeof(FakeServiceConfigurator2),
                                typeof(FakeServiceConfigurator3)))
                        .SetName("{m}(Assembly contains many ServiceConfigurators)"));

        [TestCaseSource(nameof(EnumerateServiceConfigurators_AssemblyIsValid_TestCaseData))]
        public void EnumerateServiceConfigurators_AssemblyIsValid_ResultsAreExpected(
            Assembly assembly,
            ImmutableArray<Type> configuratorTypes)
        {
            var results = ServiceConfiguratorAttribute.EnumerateServiceConfigurators(
                assembly);

            results.Select(result => result.GetType()).ShouldBeSetEqualTo(configuratorTypes);
        }

        #endregion EnumerateServiceConfigurators() Tests
    }
}
