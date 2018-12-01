using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Modix.Common.Messaging;

using Moq;
using NUnit.Framework;
using Serilog;
using Shouldly;

namespace Modix.Common.Test.Messaging
{
    [TestFixture]
    public class MediatorTests
    {
        #region TestContext

        private static (Mock<IServiceProvider> mockServiceProvider, Mediator uut) BuildTestContext()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();

            var uut = new Mediator(mockServiceProvider.Object);

            return (mockServiceProvider, uut);
        }

        #endregion TestContext

        #region Constructor Tests

        [Test]
        public void Constructor_Always_ServiceProviderIsGiven()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            uut.ServiceProvider.ShouldBeSameAs(mockServiceProvider.Object);
        }

        #endregion Constructor Tests

        #region Dispatch() Tests

        [Test]
        public async Task Dispatch_NotificationIsNull_ThrowsException()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await uut.DispatchAsync(null as INotification));
        }

        [Test]
        public async Task Dispatch_HandlersAreRegistered_CreatesServiceScope()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);

            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(mockServiceScope.Object);

            var mockScopedServiceProvider = new Mock<IServiceProvider>();
            mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(mockScopedServiceProvider.Object);

            mockScopedServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(Enumerable.Empty<INotificationHandler<INotification>>());

            var mockNotification = new Mock<INotification>();

            await uut.DispatchAsync(mockNotification.Object);

            mockServiceScopeFactory
                .ShouldHaveReceived(x => x.CreateScope());
        }

        [Test]
        public async Task Dispatch_HandlersAreRegistered_PublishesNotification()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);

            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(mockServiceScope.Object);

            var mockScopedServiceProvider = new Mock<IServiceProvider>();
            mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(mockScopedServiceProvider.Object);

            var mockHandlers = new[]
            {
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>()
            };
            mockScopedServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var mockNotification = new Mock<INotification>();

            await uut.DispatchAsync(mockNotification.Object);

            mockHandlers
                .EachShould(x => x
                    .ShouldHaveReceived(y => y
                        .HandleNotificationAsync(mockNotification.Object, default(CancellationToken))));
        }

        [Test]
        public async Task Dispatch_HandlerThrowsException_ExceptionIsLogged()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);

            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(mockServiceScope.Object);

            var mockScopedServiceProvider = new Mock<IServiceProvider>();
            mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(mockScopedServiceProvider.Object);

            var mockHandlers = new[]
            {
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>()
            };
            mockScopedServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var exception = new Exception();
            mockHandlers[1]
                .Setup(x => x.HandleNotificationAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Throws(exception);

            var mockLogger = new Mock<ILogger>();
            mockScopedServiceProvider
                .Setup(x => x.GetService(typeof(ILogger)))
                .Returns(mockLogger.Object);

            var mockNotification = new Mock<INotification>();

            await uut.DispatchAsync(mockNotification.Object);

            mockLogger
                .ShouldHaveReceived(x => x.Error(exception, It.IsAny<string>(), mockNotification.Object));
        }

        [Test]
        public async Task Dispatch_HandlerThrowsException_PublishesNotificationToOtherHandlers()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);

            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(mockServiceScope.Object);

            var mockScopedServiceProvider = new Mock<IServiceProvider>();
            mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(mockScopedServiceProvider.Object);

            var mockHandlers = new[]
            {
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>()
            };
            mockScopedServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var exception = new Exception();
            mockHandlers[1]
                .Setup(x => x.HandleNotificationAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Throws(exception);

            var mockNotification = new Mock<INotification>();

            await uut.DispatchAsync(mockNotification.Object);

            mockHandlers
                .EachShould(x => x
                    .ShouldHaveReceived(y => y
                        .HandleNotificationAsync(mockNotification.Object, default(CancellationToken))));
        }

        #endregion Dispatch() Tests

        #region PublishAsync(notification) Tests

        [Test]
        public async Task PublishAsync_NotificationIsNull_ThrowsException()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var cancellationTokenSource = new CancellationTokenSource();

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await uut.PublishAsync(null as INotification, cancellationTokenSource.Token));
        }

        [Test]
        public async Task PublishAsync_HandlersAreRegistered_PublishesNotification()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockHandlers = new[]
            {
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>(),
                new Mock<INotificationHandler<INotification>>()
            };
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(mockHandlers.Select(x => x.Object));

            var mockNotification = new Mock<INotification>();
            var cancellationTokenSource = new CancellationTokenSource();

            await uut.PublishAsync(mockNotification.Object, cancellationTokenSource.Token);

            mockHandlers
                .EachShould(x => x
                    .ShouldHaveReceived(y => y
                        .HandleNotificationAsync(mockNotification.Object, cancellationTokenSource.Token)));
        }

        [Test]
        public void PublishAsync_HandlersAreNotRegistered_CompletesImmediately()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            mockServiceProvider.Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<INotification>>)))
                .Returns(Enumerable.Empty<INotificationHandler<INotification>>());

            var mockNotification = new Mock<INotification>();
            var cancellationTokenSource = new CancellationTokenSource();

            var result = uut.PublishAsync(mockNotification.Object, cancellationTokenSource.Token);

            result.IsCompleted.ShouldBeTrue();
        }

        #endregion PublishAsync(notification) Tests

        #region PublishAsync(request) Tests

        [Test]
        public async Task PublishAsync_RequestIsNull_ThrowsException()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var cancellationTokenSource = new CancellationTokenSource();

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await uut.PublishAsync<IRequest<object>, object>(null, cancellationTokenSource.Token));
        }

        [Test]
        public async Task PublishAsync_HandlerIsRegistered_PublishesRequest()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockHandler = new Mock<IRequestHandler<IRequest<object>, object>>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IRequestHandler<IRequest<object>, object>)))
                .Returns(mockHandler.Object);

            var mockRequest = new Mock<IRequest<object>>();
            var cancellationTokenSource = new CancellationTokenSource();

            await uut.PublishAsync<IRequest<object>, object>(mockRequest.Object, cancellationTokenSource.Token);

            mockHandler
                .ShouldHaveReceived(y => y
                    .HandleRequestAsync(mockRequest.Object, cancellationTokenSource.Token));
        }

        [Test]
        public async Task PublishAsync_HandlerIsRegistered_ReturnsResponse()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            var mockHandler = new Mock<IRequestHandler<IRequest<object>, object>>();
            var handlerResult = new object();
            mockHandler
                .Setup(x => x.HandleRequestAsync(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(handlerResult));
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IRequestHandler<IRequest<object>, object>)))
                .Returns(mockHandler.Object);

            var mockRequest = new Mock<IRequest<object>>();
            var cancellationTokenSource = new CancellationTokenSource();

            var result = await uut.PublishAsync<IRequest<object>, object>(mockRequest.Object, cancellationTokenSource.Token);

            result.ShouldBeSameAs(handlerResult);
        }

        [Test]
        public async Task PublishAsync_HandlerIsNotRegistered_ThrowsException()
        {
            (var mockServiceProvider, var uut) = BuildTestContext();

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IRequestHandler<IRequest<object>, object>)))
                .Returns(null);

            var mockRequest = new Mock<IRequest<object>>();
            var cancellationTokenSource = new CancellationTokenSource();

            await Should.ThrowAsync<Exception>(async () => 
                await uut.PublishAsync<IRequest<object>, object>(mockRequest.Object, cancellationTokenSource.Token));
        }

        #endregion PublishAsync(request) Tests
    }
}
