using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Common.Messaging;

namespace Modix.Common.Test.Extensions.Microsoft.Extensions.Hosting
{
    [TestFixture]
    public class HostLifetimeNotificationBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                MockMessagePublisher = new Mock<IMessagePublisher>();

                MockServiceProvider = new Mock<IServiceProvider>();
                MockServiceProvider
                    .Setup(x => x.GetService(typeof(IMessagePublisher)))
                    .Returns(() => MockMessagePublisher.Object);

                MockServiceScope = new Mock<IServiceScope>();
                MockServiceScope
                    .Setup(x => x.ServiceProvider)
                    .Returns(() => MockServiceProvider.Object);

                MockServiceScopeFactory = new Mock<IServiceScopeFactory>();
                MockServiceScopeFactory
                    .Setup(x => x.CreateScope())
                    .Returns(() => MockServiceScope.Object);
            }

            public HostLifetimeNotificationBehavior BuildUut()
                => new HostLifetimeNotificationBehavior(
                    LoggerFactory.CreateLogger<HostLifetimeNotificationBehavior>(),
                    MockServiceScopeFactory.Object);

            public readonly Mock<IMessagePublisher> MockMessagePublisher;
            public readonly Mock<IServiceProvider> MockServiceProvider;
            public readonly Mock<IServiceScope> MockServiceScope;
            public readonly Mock<IServiceScopeFactory> MockServiceScopeFactory;
        }

        #endregion Test Context

        #region StartAsync() Tests

        [Test]
        public async Task StartAsync_Always_PublishesNotificationWithinScope()
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.StartAsync(testContext.CancellationToken);

            testContext.MockMessagePublisher.ShouldHaveReceived(x => x
                .PublishAsync(It.IsNotNull<HostStartingNotification>(), testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion StartAsync() Tests

        #region StopAsync() Tests

        [Test]
        public async Task StopAsync_Always_PublishesNotificationWithinScope()
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.StopAsync(testContext.CancellationToken);

            testContext.MockMessagePublisher.ShouldHaveReceived(x => x
                .PublishAsync(It.IsNotNull<HostStoppingNotification>(), testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion StopAsync() Tests
    }
}
