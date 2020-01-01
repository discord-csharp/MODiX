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
    public class MessageDispatcherTests
    {
        #region TestContext

        private static (Mock<IServiceScopeFactory> mockServiceScopeFactory, MessageDispatcher uut) BuildTestContext()
        {
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            var uut = new MessageDispatcher(mockServiceScopeFactory.Object);

            return (mockServiceScopeFactory, uut);
        }

        #endregion TestContext

        #region Constructor Tests

        [Test]
        public void Constructor_Always_ServiceProviderIsGiven()
        {
            (var mockServiceScopeFactory, var uut) = BuildTestContext();

            uut.ServiceScopeFactory.ShouldBeSameAs(mockServiceScopeFactory.Object);
        }

        #endregion Constructor Tests

        #region Dispatch() Tests

        [Test]
        public void Dispatch_NotificationIsNull_ThrowsException()
        {
            (var mockServiceScopeFactory, var uut) = BuildTestContext();

            Should.Throw<ArgumentNullException>(() =>
                uut.Dispatch((null as INotification)!));
        }

        [Test]
        public async Task Dispatch_HandlersAreRegistered_CreatesServiceScope()
        {
            (var mockServiceScopeFactory, var uut) = BuildTestContext();

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
            (var mockServiceScopeFactory, var uut) = BuildTestContext();

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
        public async Task Dispatch_HandlerThrowsException_PublishesNotificationToOtherHandlers()
        {
            (var mockServiceScopeFactory, var uut) = BuildTestContext();

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
    }
}
