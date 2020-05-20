#nullable enable

using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class RoleUpdatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockOldRole = new Mock<ISocketRole>();
            var mockNewRole = new Mock<ISocketRole>();

            var uut = new RoleUpdatedNotification(mockOldRole.Object, mockNewRole.Object);

            uut.OldRole.ShouldBeSameAs(mockOldRole.Object);
            uut.NewRole.ShouldBeSameAs(mockNewRole.Object);
        }

        #endregion Constructor Tests
    }
}
