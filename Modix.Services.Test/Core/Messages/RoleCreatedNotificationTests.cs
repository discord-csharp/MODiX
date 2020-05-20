#nullable enable

using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class RoleCreatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockRole = new Mock<ISocketRole>();

            var uut = new RoleCreatedNotification(mockRole.Object);

            uut.Role.ShouldBeSameAs(mockRole.Object);
        }

        #endregion Constructor Tests
    }
}
