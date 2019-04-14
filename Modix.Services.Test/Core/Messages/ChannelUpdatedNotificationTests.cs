using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class ChannelUpdatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockOldChannel = new Mock<ISocketChannel>();
            var mockNewChannel = new Mock<ISocketChannel>();

            var uut = new ChannelUpdatedNotification(mockOldChannel.Object, mockNewChannel.Object);

            uut.OldChannel.ShouldBeSameAs(mockOldChannel.Object);
            uut.NewChannel.ShouldBeSameAs(mockNewChannel.Object);
        }

        #endregion Constructor Tests
    }
}
