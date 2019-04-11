using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class MessageUpdatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockOldMessage = new Mock<ICacheable<IMessage, ulong>>();
            var mockNewMessage = new Mock<ISocketMessage>();
            var mockChannel = new Mock<IISocketMessageChannel>();

            var uut = new MessageUpdatedNotification(mockOldMessage.Object, mockNewMessage.Object, mockChannel.Object);

            uut.OldMessage.ShouldBeSameAs(mockOldMessage.Object);
            uut.NewMessage.ShouldBeSameAs(mockNewMessage.Object);
            uut.Channel.ShouldBeSameAs(mockChannel.Object);
        }

        #endregion Constructor Tests
    }
}
