using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class ChannelCreatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockChannel = new Mock<ISocketChannel>();

            var uut = new ChannelCreatedNotification(mockChannel.Object);

            uut.Channel.ShouldBeSameAs(mockChannel.Object);
        }

        #endregion Constructor Tests
    }
}
