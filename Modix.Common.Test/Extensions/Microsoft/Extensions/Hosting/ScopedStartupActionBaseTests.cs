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
    public class ScopedStartupActionBaseTests
    {
        #region Test Context

        internal class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                MockServiceScope
                    .Setup(x => x.ServiceProvider)
                    .Returns(() => MockServiceProvider.Object);

                MockServiceScopeFactory
                    .Setup(x => x.CreateScope())
                    .Returns(() => MockServiceScope.Object);
            }

            public readonly Mock<IServiceProvider> MockServiceProvider
                = new Mock<IServiceProvider>();
            public readonly Mock<IServiceScope> MockServiceScope
                = new Mock<IServiceScope>();
            public readonly Mock<IServiceScopeFactory> MockServiceScopeFactory
                = new Mock<IServiceScopeFactory>();

            public Mock<ScopedStartupActionBase> BuildMockUut()
                => new Mock<ScopedStartupActionBase>(
                    LoggerFactory.CreateLogger<ScopedStartupActionBase>(),
                    MockServiceScopeFactory.Object);
        }

        #endregion Test Context

        #region StartAsync() Tests

        [Test]
        public async Task StartAsync_Always_InvokesOnStartingAsyncAndCompletesWhenDone()
        {
            using var testContext = new TestContext();

            var mockUut = testContext.BuildMockUut();

            await mockUut.Object.StartAsync(
                testContext.CancellationToken);

            testContext.MockServiceScopeFactory.ShouldHaveReceived(x => x
                .CreateScope());

            mockUut.ShouldHaveReceived(x => x
                .OnStartingAsync(
                    testContext.MockServiceProvider.Object,
                    testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public void StopAsync_Always_DoesNothing()
        {
            using var testContext = new TestContext();

            var mockUut = testContext.BuildMockUut();

            var result = mockUut.Object.StopAsync(
                testContext.CancellationToken);

            result.IsCompletedSuccessfully.ShouldBeTrue();
        }

        #endregion StopAsync() Tests
    }
}
