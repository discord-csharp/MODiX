using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class MessageDeletedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockMessage = new Mock<ICacheable<IMessage, ulong>>();
            var mockChannel = new Mock<IISocketMessageChannel>();

            var uut = new MessageDeletedNotification(mockMessage.Object, mockChannel.Object);

            uut.Message.ShouldBeSameAs(mockMessage.Object);
            uut.Channel.ShouldBeSameAs(mockChannel.Object);
        }

        #endregion Constructor Tests
    }
}
