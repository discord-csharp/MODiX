using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Common.Messaging;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace Modix.Common.Test.Messaging
{
    [TestFixture]
    public class MessageDispatcherTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                Logger = LoggerFactory.CreateLogger<MessageDispatcher>();

                MockCancellationTokenSource = new Mock<ICancellationTokenSource>();
                MockCancellationTokenSource
                    .Setup(x => x.Token)
                    .Returns(() => CancellationToken);

                MockCancellationTokenSourceFactory = new Mock<ICancellationTokenSourceFactory>();
                MockCancellationTokenSourceFactory
                    .Setup(x => x.Create(It.IsAny<TimeSpan>()))
                    .Returns(() => MockCancellationTokenSource.Object);

                MockServiceProvider = new Mock<IServiceProvider>();

                MockServiceScope = new Mock<IServiceScope>();
                MockServiceScope
                    .Setup(x => x.ServiceProvider)
                    .Returns(() => MockServiceProvider.Object);

                MockServiceScopeFactory = new Mock<IServiceScopeFactory>();
                MockServiceScopeFactory
                    .Setup(x => x.CreateScope())
                    .Returns(() => MockServiceScope.Object);

                Options = Microsoft.Extensions.Options.Options.Create(new MessagingOptions());
            }

            public MessageDispatcher BuildUut()
                => new MessageDispatcher(
                    MockCancellationTokenSourceFactory.Object,
                    Logger,
                    Options,
                    MockServiceScopeFactory.Object);

            public readonly ILogger<MessageDispatcher> Logger;

            public readonly Mock<ICancellationTokenSource> MockCancellationTokenSource;
            public readonly Mock<ICancellationTokenSourceFactory> MockCancellationTokenSourceFactory;
            public readonly Mock<IServiceProvider> MockServiceProvider;
            public readonly Mock<IServiceScope> MockServiceScope;
            public readonly Mock<IServiceScopeFactory> MockServiceScopeFactory;

            public readonly IOptions<MessagingOptions> Options;
        }

        #endregion Test Context

        #region DispatchAsync() Tests

        public static readonly ImmutableArray<TestCaseData> DispatchAsync_TestCaseData
            = ImmutableArray.Create(
                /*                  dispatchTimeout,            handlerCount,   timeout,                    cancellationTokenSourceDelay    */
                new TestCaseData(   TimeSpan.Zero,              0,              TimeSpan.Zero,              TimeSpan.Zero                   ).SetName("{m}(No handlers registered)"),
                new TestCaseData(   TimeSpan.Zero,              1,              TimeSpan.Zero,              TimeSpan.Zero                   ).SetName("{m}(One handler registered)"),
                new TestCaseData(   TimeSpan.Zero,              3,              TimeSpan.Zero,              TimeSpan.Zero                   ).SetName("{m}(Many handlers registered)"),
                new TestCaseData(   TimeSpan.FromSeconds(1),    1,              null,                       TimeSpan.FromSeconds(1)         ).SetName("{m}(Timeout not given)"),
                new TestCaseData(   TimeSpan.FromSeconds(2),    1,              TimeSpan.FromSeconds(3),    TimeSpan.FromSeconds(3)         ).SetName("{m}(Timeout given)"));

        [TestCaseSource(nameof(DispatchAsync_TestCaseData))]
        public async Task DispatchAsync_Always_InvokesHandlersInServiceScopeWithTimeout(
            TimeSpan dispatchTimeout,
            int handlerCount,
            TimeSpan? timeout,
            TimeSpan cancellationTokenSourceDelay)
        {
            using var testContext = new TestContext();
            testContext.Options.Value.DispatchTimeout = dispatchTimeout;

            var mockHandlers = Enumerable.Range(0, handlerCount)
                .Select(_ => new Mock<INotificationHandler<object>>())
                .ToArray();

            testContext.MockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<object>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var uut = testContext.BuildUut();

            var notification = new object();

            await uut.DispatchAsync(notification, timeout);

            testContext.MockCancellationTokenSourceFactory.ShouldHaveReceived(x => x
                .Create(cancellationTokenSourceDelay));

            testContext.MockServiceScopeFactory.ShouldHaveReceived(x => x
                .CreateScope());

            foreach (var mockHandler in mockHandlers)
                mockHandler.ShouldHaveReceived(x => x
                    .HandleNotificationAsync(notification, testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
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

            await uut.DispatchAsync(mockNotification.Object);

            mockNotification.ShouldHaveReceived(x => x
                .BeginLogScope(testContext.Logger));

            mockNotificationLogScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        public static readonly ImmutableArray<TestCaseData> DispatchAsync_HandlerThrowsException_TestCaseData
            = ImmutableArray.Create(
                /*                  handlerCount,   handlerExceptionIndex   */
                new TestCaseData(   1,              0                       ).SetName("{m}(Single handler)"),
                new TestCaseData(   3,              0                       ).SetName("{m}(First handler)"),
                new TestCaseData(   3,              1                       ).SetName("{m}(Second handler)"),
                new TestCaseData(   3,              2                       ).SetName("{m}(Third handler)"));

        [TestCaseSource(nameof(DispatchAsync_HandlerThrowsException_TestCaseData))]
        public async Task DispatchAsync_HandlerThrowsException_InvokesOtherHandlers(
            int handlerCount,
            int handlerExceptionIndex)
        {
            using var testContext = new TestContext();

            var mockHandlers = Enumerable.Range(0, handlerCount)
                .Select(_ => new Mock<INotificationHandler<object>>())
                .ToArray();

            testContext.MockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<object>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var exception = new Exception();
            mockHandlers[handlerExceptionIndex]
                .Setup(x => x.HandleNotificationAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Throws(exception);

            var uut = testContext.BuildUut();

            var notification = new object();

            await uut.DispatchAsync(notification);

            foreach (var mockHandler in mockHandlers)
                mockHandler.ShouldHaveReceived(x => x
                    .HandleNotificationAsync(notification, testContext.CancellationToken));

            testContext.MockServiceScope.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion DispatchAsync() Tests
    }
}
