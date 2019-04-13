using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class GuildAvailableNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockGuild = new Mock<ISocketGuild>();

            var uut = new GuildAvailableNotification(mockGuild.Object);

            uut.Guild.ShouldBeSameAs(mockGuild.Object);
        }

        #endregion Constructor Tests
    }
}
