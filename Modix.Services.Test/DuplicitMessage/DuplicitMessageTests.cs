using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Services.DuplicitMessage;
using Modix.Services.Moderation;
using Modix.Services.Test.Moderation;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

namespace Modix.Services.Test.DuplicitMessage
{
    [TestFixture]
    public class DuplicitMessageTests
    {
        private static AutoMocker CreateMocker()
        {
            var autoMocker = new AutoMocker();

            var mockSelfUser = autoMocker.GetMock<ISocketSelfUser>();
            mockSelfUser
                .Setup(x => x.Id)
                .Returns(1);

            autoMocker.GetMock<ISelfUserProvider>()
                      .Setup(x => x.GetSelfUserAsync(It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult(mockSelfUser.Object));

            return autoMocker;
        }

        private static ISocketMessage BuildTestMessage(AutoMocker autoMocker, string content, DateTimeOffset timestamp)
        {
            var mockMessage = autoMocker.GetMock<ISocketMessage>();

            var mockGuild = autoMocker.GetMock<IGuild>();

            var mockAuthor = autoMocker.GetMock<IGuildUser>();
            mockAuthor
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockAuthor
                .Setup(x => x.Id)
                .Returns(2);

            mockMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);

            var mockChannel = autoMocker.GetMock<IMessageChannel>();
            mockChannel
                .As<IGuildChannel>()
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockMessage
                .Setup(x => x.Channel)
                .Returns(mockChannel.Object);

            mockMessage
                .Setup(x => x.Content)
                .Returns(content);

            mockMessage
                .Setup(x => x.Timestamp)
                .Returns(timestamp);

            return mockMessage.Object;
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessagesNotEqual_DoesNotDeleteMessage()
        {
            var autoMocker = CreateMocker();
            var uut = autoMocker.CreateInstance<DuplicitMessageHandler>();

            autoMocker.GetMock<ISocketMessage>()
                      .Setup(x => x.Author)
                      .Returns(autoMocker.Get<IUser>());

            var now = new DateTime(2019, 1, 1, 1, 1, 1);
            var addSecs = 1;

            var notification1 = new MessageReceivedNotification(BuildTestMessage(CreateMocker(), "abcde12345", now));
            var notification2 = new MessageReceivedNotification(BuildTestMessage(autoMocker, "lololololol", now.AddSeconds(addSecs)));

            await uut.HandleNotificationAsync(notification1);
            autoMocker.MessageShouldNotHaveBeenDeleted();

            await uut.HandleNotificationAsync(notification2);
            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_SlowMessagesEqual_DeletesSecondMessage()
        {
            var autoMocker = CreateMocker();
            var uut = autoMocker.CreateInstance<DuplicitMessageHandler>();

            autoMocker.GetMock<ISocketMessage>()
                      .Setup(x => x.Author)
                      .Returns(autoMocker.Get<IUser>());

            var now = new DateTime(2019, 1, 1, 1, 1, 1);
            var addSecs = uut.MinimumSecondsBetweenMessages + 0.1;

            var notification1 = new MessageReceivedNotification(BuildTestMessage(CreateMocker(), "abcde12345", now));
            var notification2 = new MessageReceivedNotification(BuildTestMessage(autoMocker, "abcde12345", now.AddSeconds(addSecs)));

            await uut.HandleNotificationAsync(notification1);
            autoMocker.MessageShouldNotHaveBeenDeleted();

            await uut.HandleNotificationAsync(notification2);
            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_FastMessagesEqual_DeletesSecondMessage()
        {
            var autoMocker = CreateMocker();
            var uut = autoMocker.CreateInstance<DuplicitMessageHandler>();

            autoMocker.GetMock<ISocketMessage>()
                      .Setup(x => x.Author)
                      .Returns(autoMocker.Get<IUser>());

            var now = new DateTime(2019, 1, 1, 1, 1, 1);
            var addSecs = uut.MinimumSecondsBetweenMessages - 0.1;

            var notification1 = new MessageReceivedNotification(BuildTestMessage(CreateMocker(), "abcde12345", now));
            var notification2 = new MessageReceivedNotification(BuildTestMessage(autoMocker, "abcde12345", now.AddSeconds(addSecs)));

            await uut.HandleNotificationAsync(notification1);
            autoMocker.MessageShouldNotHaveBeenDeleted();

            await uut.HandleNotificationAsync(notification2);
            autoMocker.MessageShouldHaveBeenDeleted();
        }
    }


    internal static class DuplicitMessageHandlerAssertions
    {
        public static void MessageShouldHaveBeenDeleted(this AutoMocker autoMocker)
        {
            autoMocker.GetMock<IModerationService>()
                      .ShouldHaveReceived(x => x.DeleteMessageAsync(
                                              It.IsAny<IMessage>(),
                                              It.IsAny<string>(),
                                              It.IsAny<ulong>()));

            autoMocker.GetMock<IMessageChannel>()
                      .ShouldHaveReceived(x => x.SendMessageAsync(
                                              It.IsAny<string>(),
                                              It.IsAny<bool>(),
                                              It.IsAny<Embed>(),
                                              It.IsAny<RequestOptions>()));
        }
    }
}
