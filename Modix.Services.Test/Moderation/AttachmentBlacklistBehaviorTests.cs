#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Test;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Moderation;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace Modix.Services.Test.Moderation
{
    [TestFixture]
    public class AttachmentBlacklistBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext(
                ulong selfUserId,
                bool channelIsUnmoderated)
            {
                MockDesignatedChannelService = new Mock<IDesignatedChannelService>();
                MockDesignatedChannelService
                    .Setup(x => x.ChannelHasDesignationAsync(
                        It.IsAny<IGuild>(),
                        It.IsAny<IChannel>(),
                        DesignatedChannelType.Unmoderated,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(channelIsUnmoderated);

                MockModerationService = new Mock<IModerationService>();

                MockCurrentUser = new Mock<ISocketSelfUser>();
                MockCurrentUser
                    .Setup(x => x.Id)
                    .Returns(selfUserId);

                MockDiscordSocketClient = new Mock<IDiscordSocketClient>();
                MockDiscordSocketClient
                    .Setup(x => x.CurrentUser)
                    .Returns(() => MockCurrentUser.Object);
            }

            public AttachmentBlacklistBehavior BuildUut()
                => new AttachmentBlacklistBehavior(
                    MockDesignatedChannelService.Object,
                    MockDiscordSocketClient.Object,
                    LoggerFactory.CreateLogger<AttachmentBlacklistBehavior>(),
                    MockModerationService.Object);

            public readonly Mock<IDesignatedChannelService> MockDesignatedChannelService;
            public readonly Mock<IDiscordSocketClient> MockDiscordSocketClient;
            public readonly Mock<IModerationService> MockModerationService;
            public readonly Mock<ISocketSelfUser> MockCurrentUser;
        }

        #endregion Test Context

        #region HandleNotificationAsync() Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync(
            ulong selfUserId,
            ulong? guildId,
            (ulong id, bool isUnmoderated) channel,
            (ulong id, bool isBot, bool isWebhook) author,
            ulong messageId,
            IReadOnlyList<string> attachmentFilenames,
            IReadOnlyList<string> suspiciousFilenames)
        {
            var mockChannel = new Mock<IMessageChannel>();
            mockChannel
                .Setup(x => x.Id)
                .Returns(channel.id);

            var mockAuthor = new Mock<IUser>();
            mockAuthor
                .Setup(x => x.Id)
                .Returns(author.id);
            mockAuthor
                .Setup(x => x.IsBot)
                .Returns(author.isBot);
            mockAuthor
                .Setup(x => x.IsWebhook)
                .Returns(author.isWebhook);
            mockAuthor
                .Setup(x => x.Mention)
                .Returns($"<@{author.id}>");

            if (guildId.HasValue)
            {
                var mockGuild = new Mock<ISocketGuild>();
                mockGuild
                    .Setup(x => x.Id)
                    .Returns(guildId.Value);

                mockChannel.As<ISocketGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);

                mockAuthor.As<IGuildUser>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);
            }

            var mockMessage = new Mock<ISocketMessage>();
            mockMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockMessage
                .Setup(x => x.Channel)
                .Returns(mockChannel.Object);
            mockMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);
            mockMessage
                .Setup(x => x.Attachments)
                .Returns(attachmentFilenames
                    .Select(attachmentFilename =>
                    {
                        var mockAttachment = new Mock<IAttachment>();
                        mockAttachment
                            .Setup(x => x.Filename)
                            .Returns(attachmentFilename);

                        return mockAttachment.Object;
                    })
                    .ToArray());

            var notification = new MessageReceivedNotification(
                mockMessage.Object);

            return new TestCaseData(selfUserId, mockChannel, notification, channel.isUnmoderated, suspiciousFilenames);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageHasNoSuspiciousAttachments_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: default,        guildId: default,           channel: (id: default,          isUnmoderated: default),    author: (id: default,           isBot: default, isWebhook: default),    messageId: default,         attachmentFilenames: Array.Empty<string>(),                     suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: ulong.MinValue, guildId: ulong.MinValue,    channel: (id: ulong.MinValue,   isUnmoderated: false),      author: (id: ulong.MinValue,    isBot: false,   isWebhook: false),      messageId: ulong.MinValue,  attachmentFilenames: Array.Empty<string>(),                     suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: ulong.MaxValue, guildId: ulong.MaxValue,    channel: (id: ulong.MaxValue,   isUnmoderated: true),       author: (id: ulong.MaxValue,    isBot: true,    isWebhook: true),       messageId: ulong.MaxValue,  attachmentFilenames: Array.Empty<string>(),                     suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 1,              guildId: null,              channel: (id: 2,                isUnmoderated: false),      author: (id: 3,                 isBot: false,   isWebhook: false),      messageId: 4,               attachmentFilenames: new[] { "5.exe" },                         suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Message is not from guild)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 6,              guildId: 7,                 channel: (id: 8,                isUnmoderated: true),       author: (id: 9,                 isBot: false,   isWebhook: false),      messageId: 10,              attachmentFilenames: new[] { "11.exe" },                        suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Channel is unmoderated)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 12,             guildId: 13,                channel: (id: 14,               isUnmoderated: false),      author: (id: 15,                isBot: true,    isWebhook: false),      messageId: 16,              attachmentFilenames: new[] { "17.exe" },                        suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Author is bot)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 18,             guildId: 19,                channel: (id: 20,               isUnmoderated: false),      author: (id: 21,                isBot: false,   isWebhook: true),       messageId: 22,              attachmentFilenames: new[] { "23.exe" },                        suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Author is webhook)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 24,             guildId: 25,                channel: (id: 26,               isUnmoderated: false),      author: (id: 27,                isBot: false,   isWebhook: false),      messageId: 28,              attachmentFilenames: Array.Empty<string>(),                     suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Message has no attachments)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 29,             guildId: 30,                channel: (id: 31,               isUnmoderated: false),      author: (id: 32,                isBot: false,   isWebhook: false),      messageId: 33,              attachmentFilenames: new[] { "34.txt" },                        suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Attachment is not suspicious)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 35,             guildId: 36,                channel: (id: 37,               isUnmoderated: false),      author: (id: 38,                isBot: false,   isWebhook: false),      messageId: 39,              attachmentFilenames: new[] { "40.jpg", "41.bpm", "42.png" },    suspiciousFilenames: Array.Empty<string>()  ).SetName("{m}(Attachments are not suspicious)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageHasNoSuspiciousAttachments_TestCaseData))]
        public async Task HandleNotificationAsync_MessageHasNoSuspiciousAttachments_IgnoresMessage(
            ulong selfUserId,
            Mock<IMessageChannel> mockChannel,
            MessageReceivedNotification notification,
            bool channelIsUnmoderated,
            IReadOnlyList<string> suspiciousFilenames)
        {
            using var testContext = new TestContext(
                selfUserId,
                channelIsUnmoderated);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            var guild = (notification.Message.Channel as ISocketGuildChannel)?.Guild;
            if (guild is null)
                testContext.MockDesignatedChannelService.ShouldNotHaveReceived(x => x
                    .ChannelHasDesignationAsync(
                        It.IsAny<IGuild>(),
                        It.IsAny<IChannel>(),
                        It.IsAny<DesignatedChannelType>(),
                        It.IsAny<CancellationToken>()));
            else
                testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                        .ChannelHasDesignationAsync(
                            guild,
                            mockChannel.Object,
                            DesignatedChannelType.Unmoderated,
                            testContext.CancellationToken),
                    Times.AtMostOnce());

            testContext.MockModerationService.Invocations.ShouldBeEmpty();

            mockChannel.ShouldNotHaveReceived(x => x
                .SendMessageAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<Embed>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<AllowedMentions?>()));
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageHasAnySuspiciousAttachments_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 1,  guildId: 2,     channel: (id: 3,    isUnmoderated: false),  author: (id: 4,     isBot: false,   isWebhook: false),  messageId: 5,   attachmentFilenames: new[] { "6.exe" },                         suspiciousFilenames: new[] { "6.exe" }                      ).SetName("{m}(Message has suspicious attachment)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 7,  guildId: 8,     channel: (id: 9,    isUnmoderated: false),  author: (id: 10,    isBot: false,   isWebhook: false),  messageId: 11,  attachmentFilenames: new[] { "12.dll", "13.bat", "14.sh" },     suspiciousFilenames: new[] { "12.dll", "13.bat", "14.sh" }  ).SetName("{m}(Message has suspicious attachments)"),
                BuildTestCaseData_HandleNotificationAsync(  selfUserId: 15, guildId: 16,    channel: (id: 17,   isUnmoderated: false),  author: (id: 18,    isBot: false,   isWebhook: false),  messageId: 19,  attachmentFilenames: new[] { "20.msc", "21.txt", "22.jar" },    suspiciousFilenames: new[] { "20.msc", "22.jar" }           ).SetName("{m}(Message has suspicious and non-suspicious attachments)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageHasAnySuspiciousAttachments_TestCaseData))]
        public async Task HandleNotificationAsync_MessageHasAnySuspiciousAttachments_DeletesMessageAndReplies(
            ulong selfUserId,
            Mock<IMessageChannel> mockChannel,
            MessageReceivedNotification notification,
            bool channelIsUnmoderated,
            IReadOnlyList<string> suspiciousFilenames)
        {
            using var testContext = new TestContext(
                selfUserId,
                channelIsUnmoderated);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    ((ISocketGuildChannel)notification.Message.Channel).Guild,
                    mockChannel.Object,
                    DesignatedChannelType.Unmoderated,
                    testContext.CancellationToken));

            testContext.MockModerationService.ShouldHaveReceived(x => x
                .DeleteMessageAsync(
                    notification.Message,
                    It.IsNotNull<string>(),
                    selfUserId,
                    testContext.CancellationToken));

            var reason = testContext.MockModerationService.Invocations
                .Where(x => x.Method.Name == nameof(IModerationService.DeleteMessageAsync))
                .Select(x => (string)x.Arguments[1])
                .First();
            reason.ShouldContain("attach");
            reason.ShouldContain("suspicious");
            foreach(var suspiciousFilename in suspiciousFilenames)
                reason.ShouldContain(suspiciousFilename);

            mockChannel.ShouldHaveReceived(x => x
                .SendMessageAsync(
                    It.Is<string>(y => (y != null) && y.Contains(notification.Message.Author.Id.ToString())),
                    It.IsAny<bool>(),
                    It.IsAny<Embed>(),
                    It.Is<RequestOptions>(y => (y != null) && (y.CancelToken == testContext.CancellationToken)),
                    null));
        }

        #endregion HandleNotificationAsync() Tests
    }
}
