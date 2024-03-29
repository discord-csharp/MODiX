using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Common.Messaging;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace Modix.Common.Test.Messaging
{
    [TestFixture]
    public class MessagePublisherTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                Logger = LoggerFactory.CreateLogger<MessagePublisher>();

                MockServiceProvider = new Mock<IServiceProvider>();
            }

            public MessagePublisher BuildUut()
                => new MessagePublisher(
                    Logger,
                    MockServiceProvider.Object);

            public readonly ILogger<MessagePublisher> Logger;

            public readonly Mock<IServiceProvider> MockServiceProvider;
        }

        #endregion Test Context

        #region PublishAsync() Tests

        public static readonly ImmutableArray<TestCaseData> PublishAsync_TestCaseData
            = ImmutableArray.Create(
                /*                  handlerCount    */
                new TestCaseData(   0               ).SetName("{m}(No handlers)"),
                new TestCaseData(   1               ).SetName("{m}(Single handler)"),
                new TestCaseData(   3               ).SetName("{m}(Many handlers)"));

        [TestCaseSource(nameof(PublishAsync_TestCaseData))]
        public async Task PublishAsync_Always_InvokesHandlers(
            int handlerCount)
        {
            using var testContext = new TestContext();

            var mockHandlers = Enumerable.Range(0, handlerCount)
                .Select(_ => new Mock<INotificationHandler<object>>())
                .ToArray();

            testContext.MockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<object>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var uut = testContext.BuildUut();

            var notification = new object();

            await uut.PublishAsync(notification, testContext.CancellationToken);

            foreach(var mockHandler in mockHandlers)
                mockHandler.ShouldHaveReceived(x => x
                    .HandleNotificationAsync(notification, testContext.CancellationToken));
        }

        [Test]
        public async Task DispatchAsync_NotificationIsLogScopeProvider_CreatesLogScope()
        {
            using var testContext = new TestContext();

            var mockHandler = new Mock<INotificationHandler<object>>();

            testContext.MockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<object>>)))
                .Returns(EnumerableEx.From(mockHandler.Object));

            var uut = testContext.BuildUut();

            var mockNotificationLogScope = new Mock<IDisposable>();

            var mockNotification = new Mock<ILogScopeProvider>();
            mockNotification
                .Setup(x => x.BeginLogScope(It.IsAny<ILogger>()))
                .Returns(() => mockNotificationLogScope.Object);

            await uut.PublishAsync(mockNotification.Object as object, testContext.CancellationToken);

            mockNotification.ShouldHaveReceived(x => x
                .BeginLogScope(testContext.Logger));

            mockNotificationLogScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion PublishAsync() Tests
    }
}
