using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class UserLeftNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockGuildUser = new Mock<ISocketGuildUser>();

            var uut = new UserLeftNotification(mockGuildUser.Object);

            uut.GuildUser.ShouldBeSameAs(mockGuildUser.Object);
        }

        #endregion Constructor Tests
    }
}
