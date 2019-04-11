using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class MessageReceivedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockMessage = new Mock<ISocketMessage>();

            var uut = new MessageReceivedNotification(mockMessage.Object);

            uut.Message.ShouldBeSameAs(mockMessage.Object);
        }

        #endregion Constructor Tests
    }
}
