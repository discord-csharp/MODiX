#nullable enable

using Moq;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class GuildMemberUpdatedNotificationTests
    {
        #region Constructor Tests

        [Test]
        public void Constructor_Always_PropertiesAreGiven()
        {
            var mockOldMember = new Mock<ISocketGuildUser>();
            var mockNewMember = new Mock<ISocketGuildUser>();

            var uut = new GuildMemberUpdatedNotification(mockOldMember.Object, mockNewMember.Object);

            uut.OldMember.ShouldBeSameAs(mockOldMember.Object);
            uut.NewMember.ShouldBeSameAs(mockNewMember.Object);
        }

        #endregion Constructor Tests
    }
}
