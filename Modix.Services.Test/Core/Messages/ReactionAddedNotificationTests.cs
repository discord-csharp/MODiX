using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class ReactionAddedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockMessage = new Mock<ICacheable<IUserMessage, ulong>>();
            var mockChannel = new Mock<IISocketMessageChannel>();
            var mockReaction = new Mock<ISocketReaction>();

            var uut = new ReactionAddedNotification(mockMessage.Object, mockChannel.Object, mockReaction.Object);

            uut.Message.ShouldBeSameAs(mockMessage.Object);
            uut.Channel.ShouldBeSameAs(mockChannel.Object);
            uut.Reaction.ShouldBeSameAs(mockReaction.Object);
        }

        #endregion Constructor Tests
    }
}
