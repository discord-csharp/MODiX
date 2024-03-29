using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;
using NUnit.Framework;
using Shouldly;

namespace Modix.Common.Test.Extensions.Microsoft.Extensions.Hosting
{
    [TestFixture]
    public class ScopedBehaviorBaseTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                MockServiceProvider = new Mock<IServiceProvider>();

                MockServiceScope = new Mock<IServiceScope>();
                MockServiceScope
                    .Setup(x => x.ServiceProvider)
                    .Returns(() => MockServiceProvider.Object);

                MockServiceScopeFactory = new Mock<IServiceScopeFactory>();
                MockServiceScopeFactory
                    .Setup(x => x.CreateScope())
                    .Returns(() => MockServiceScope.Object);
            }

            public Mock<ScopedBehaviorBase> BuildMockUut()
                => new Mock<ScopedBehaviorBase>(
                    LoggerFactory.CreateLogger<ScopedBehaviorBase>(),
                    MockServiceScopeFactory.Object)
                {
                    CallBase = true
                };

            public readonly Mock<IServiceProvider> MockServiceProvider;
            public readonly Mock<IServiceScope> MockServiceScope;
            public readonly Mock<IServiceScopeFactory> MockServiceScopeFactory;
        }

        #endregion Test Context

        #region StartAsync() Tests

        [Test]
        public async Task StartAsync_Always_CreatesScopeAndInvokesOnStartingAsync()
        {
            using var testContext = new TestContext();

            var mockUut = testContext.BuildMockUut();

            await mockUut.Object.StartAsync(testContext.CancellationToken);

            mockUut.ShouldHaveReceived(x => x
                .OnStartingAsync(testContext.MockServiceProvider.Object, testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public async Task StopAsync_Always_CreatesScopeAndInvokesOnStoppingAsync()
        {
            using var testContext = new TestContext();

            var mockUut = testContext.BuildMockUut();

            await mockUut.Object.StopAsync(testContext.CancellationToken);

            mockUut.ShouldHaveReceived(x => x
                .OnStoppingAsync(testContext.MockServiceProvider.Object, testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion StopAsync() Tests
    }
}
