using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class UserBannedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockUser = new Mock<ISocketUser>();
            var mockGuild = new Mock<ISocketGuild>();

            var uut = new UserBannedNotification(mockUser.Object, mockGuild.Object);

            uut.User.ShouldBeSameAs(mockUser.Object);
            uut.Guild.ShouldBeSameAs(mockGuild.Object);
        }

        #endregion Constructor Tests
    }
}
