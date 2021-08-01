#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.MessageLogging;
using Modix.Services.Utilities;

using Modix.Common.Test;

namespace Modix.Services.Test.MessageLogging
{
    [TestFixture]
    public class MessageLoggingBehaviorTests
    {
        #region Test Context

        public class TestContext : AsyncMethodWithLoggerTestContext
        {
            public TestContext(
                ulong selfUserId,
                bool isChannelUnmoderated,
                IReadOnlyList<ulong> messageLogChannelIds)
            {
                MockDesignatedChannelService = new Mock<IDesignatedChannelService>();
                MockDesignatedChannelService
                    .Setup(x => x.ChannelHasDesignationAsync(
                        It.IsAny<ulong>(),
                        It.IsAny<ulong>(),
                        DesignatedChannelType.Unmoderated,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(isChannelUnmoderated);
                MockDesignatedChannelService
                    .Setup(x => x.GetDesignatedChannelsAsync(It.IsAny<IGuild>(), DesignatedChannelType.MessageLog))
                    .ReturnsAsync(() => MockMessageLogChannels.Select(x => x.Object).ToArray());

                MockMessageLogChannels = messageLogChannelIds
                    .Select(id =>
                    {
                        var mockLogMessage = new Mock<IUserMessage>();

                        var mockMessageLogChannel = new Mock<IMessageChannel>();
                        mockMessageLogChannel
                            .Setup(x => x.Id)
                            .Returns(id);
                        mockMessageLogChannel
                            .Setup(x => x.SendMessageAsync(
                                It.IsAny<string?>(),
                                It.IsAny<bool>(),
                                It.IsAny<Embed?>(),
                                It.IsAny<RequestOptions?>(),
                                It.IsAny<AllowedMentions?>(),
                                It.IsAny<MessageReference?>()))
                            .ReturnsAsync(mockLogMessage.Object);

                        return mockMessageLogChannel;
                    })
                    .ToImmutableArray();

                MockCurrentUser = new Mock<ISocketSelfUser>();
                MockCurrentUser
                    .Setup(x => x.Id)
                    .Returns(selfUserId);

                MockDiscordSocketClient = new Mock<IDiscordSocketClient>();
                MockDiscordSocketClient
                    .Setup(x => x.CurrentUser)
                    .Returns(() => MockCurrentUser.Object);
            }

            public MessageLoggingBehavior BuildUut()
                => new MessageLoggingBehavior(
                    MockDesignatedChannelService.Object,
                    MockDiscordSocketClient.Object,
                    LoggerFactory.CreateLogger<MessageLoggingBehavior>());

            public readonly Mock<IDesignatedChannelService> MockDesignatedChannelService;
            public readonly Mock<IDiscordSocketClient> MockDiscordSocketClient;
            public readonly ImmutableArray<Mock<IMessageChannel>> MockMessageLogChannels;
            public readonly Mock<ISocketSelfUser> MockCurrentUser;
        }

        #endregion Test Context

        #region HandleNotificationAsync(MessageDeletedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            ulong messageId,
            (ulong authorId, string content, IReadOnlyList<(string filename, int size)> attachments)? message,
            ulong channelId,
            ulong? guildId)
        {
            var mockCacheableMessage = new Mock<ICacheable<IMessage, ulong>>();
            mockCacheableMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockCacheableMessage
                .Setup(x => x.HasValue)
                .Returns(message.HasValue);
            if (message.HasValue)
            {
                var mockAuthor = new Mock<IUser>();
                mockAuthor
                    .Setup(x => x.Id)
                    .Returns(message.Value.authorId);

                var mockAttachments = message.Value.attachments
                    .Select(x =>
                    {
                        var mockAttachment = new Mock<IAttachment>();
                        mockAttachment
                            .Setup(y => y.Filename)
                            .Returns(x.filename);
                        mockAttachment
                            .Setup(y => y.Size)
                            .Returns(x.size);

                        return mockAttachment;
                    })
                    .ToArray();

                var mockMessage = new Mock<IMessage>();
                mockMessage
                    .Setup(x => x.Author)
                    .Returns(mockAuthor.Object);
                mockMessage
                    .Setup(x => x.Content)
                    .Returns(message.Value.content);
                mockMessage
                    .Setup(x => x.Attachments)
                    .Returns(mockAttachments
                        .Select(x => x.Object)
                        .ToArray());

                mockCacheableMessage
                    .Setup(x => x.Value)
                    .Returns(mockMessage.Object);
            }

            var mockChannel = new Mock<IISocketMessageChannel>();
            mockChannel
                .Setup(x => x.Id)
                .Returns(channelId);

            Mock<ISocketGuild>? mockGuild = null;
            if (guildId.HasValue)
            {
                mockGuild = new Mock<ISocketGuild>();
                mockGuild
                    .Setup(x => x.Id)
                    .Returns(guildId.Value);

                mockChannel.As<ISocketGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);
            }

            var notification = new MessageDeletedNotification(
                mockCacheableMessage.Object,
                mockChannel.Object);

            return new TestCaseData(selfUserId, isChannelUnmoderated, messageLogChannelIds, notification, mockChannel, mockGuild);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: default,           isChannelUnmoderated: default,  messageLogChannelIds: Array.Empty<ulong>(),     messageId: default,         message: default,                                                                                                   channelId: default,         guildId: default        ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: ulong.MinValue,    isChannelUnmoderated: false,    messageLogChannelIds: new[] { ulong.MinValue }, messageId: ulong.MinValue,  message: (authorId: ulong.MinValue, content: string.Empty,  attachments: new[] { (string.Empty, int.MinValue) }),   channelId: ulong.MinValue,  guildId: ulong.MinValue ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: ulong.MaxValue,    isChannelUnmoderated: true,     messageLogChannelIds: new[] { ulong.MaxValue }, messageId: ulong.MaxValue,  message: (authorId: ulong.MaxValue, content: string.Empty,  attachments: new[] { (string.Empty, int.MaxValue) }),   channelId: ulong.MaxValue,  guildId: ulong.MaxValue ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 1UL,               isChannelUnmoderated: false,    messageLogChannelIds: new[] { 2UL },            messageId: 3UL,             message: (authorId: 4UL,            content: "5",           attachments: Array.Empty<(string, int)>()),             channelId: 6UL,             guildId: null           ).SetName("{m}(Message is not from Guild)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 8UL,               isChannelUnmoderated: false,    messageLogChannelIds: new[] { 9UL },            messageId: 10UL,            message: (authorId: 8UL,            content: "11",          attachments: new[] { ("12", 13) }),                     channelId: 14UL,            guildId: 15UL           ).SetName("{m}(Message is from Self)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 16UL,              isChannelUnmoderated: true,     messageLogChannelIds: new[] { 17UL },           messageId: 18UL,            message: (authorId: 19UL,           content: "20",          attachments: new[] { ("21", 22) }),                     channelId: 23UL,            guildId: 24UL           ).SetName("{m}(Channel is Unmoderated)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 25UL,              isChannelUnmoderated: false,    messageLogChannelIds: Array.Empty<ulong>(),     messageId: 26UL,            message: (authorId: 27UL,           content: "28",          attachments: new[] { ("29", 30) }),                     channelId: 31UL,            guildId: 32UL           ).SetName("{m}(No MessageLog channels designated)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_IgnoresMessage(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            MessageDeletedNotification notification,
            Mock<IISocketMessageChannel> mockChannel,
            Mock<ISocketGuild>? mockGuild)
        {
            using var testContext = new TestContext(
                selfUserId,
                isChannelUnmoderated,
                messageLogChannelIds);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(notification, testContext.CancellationToken);

            if (mockGuild is null)
                testContext.MockDesignatedChannelService.Invocations.ShouldBeEmpty();
            else
            {
                testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                        .ChannelHasDesignationAsync(
                            mockGuild.Object.Id,
                            mockChannel.Object.Id,
                            DesignatedChannelType.Unmoderated,
                            testContext.CancellationToken),
                    Times.AtMostOnce());
                testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                        .GetDesignatedChannelsAsync(
                            mockGuild.Object,
                            DesignatedChannelType.MessageLog),
                    Times.AtMostOnce());
            }

            foreach(var mockMessageLogChannel in testContext.MockMessageLogChannels)
                mockMessageLogChannel.Invocations.ShouldBeEmpty();

            mockChannel.ShouldNotHaveReceived(x => x
                .SendMessageAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<Embed?>(),
                    It.IsAny<RequestOptions?>(),
                    It.IsAny<AllowedMentions?>(),
                    It.IsAny<MessageReference?>()));
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 1UL,   isChannelUnmoderated: false,    messageLogChannelIds: new[] { 2UL },                messageId: 3UL,     message: (authorId: 4UL,    content: "5",                       attachments: Array.Empty<(string, int)>()),     channelId: 6UL,     guildId: 7UL    ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 8UL,   isChannelUnmoderated: false,    messageLogChannelIds: new[] { 9UL, 10UL },          messageId: 11UL,    message: (authorId: 12UL,   content: "13",                      attachments: new[] { ("14", 15) }),             channelId: 16UL,    guildId: 17UL   ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 18UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 19UL, 20UL, 21UL },   messageId: 22UL,    message: (authorId: 23UL,   content: "24",                      attachments: new[] { ("25", 26), ("27", 28) }), channelId: 29UL,    guildId: 30UL   ).SetName("{m}(Unique Values 3)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 31UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 32UL },               messageId: 33UL,    message: null,                                                                                                  channelId: 34UL,    guildId: 35UL   ).SetName("{m}(Message is not cached)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(selfUserId: 36UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 37UL },               messageId: 38UL,    message: (authorId: 38UL,   content: new string('A', 2000),     attachments: Array.Empty<(string, int)>()),     channelId: 39UL,    guildId: 40UL   ).SetName("{m}(Message content is max length)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_LogsDeletedMessageToMessageLogChannels(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            MessageDeletedNotification notification,
            Mock<IISocketMessageChannel> mockChannel,
            Mock<ISocketGuild> mockGuild)
        {
            using var testContext = new TestContext(
                selfUserId,
                isChannelUnmoderated,
                messageLogChannelIds);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(notification, testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    mockGuild.Object.Id,
                    mockChannel.Object.Id,
                    DesignatedChannelType.Unmoderated,
                    testContext.CancellationToken));
            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .GetDesignatedChannelsAsync(
                    mockGuild.Object,
                    DesignatedChannelType.MessageLog));

            foreach (var mockMessageLogChannel in testContext.MockMessageLogChannels)
            {
                mockMessageLogChannel.ShouldHaveReceived(x => x
                    .SendMessageAsync(
                        It.IsNotNull<string>(),
                        false,
                        It.IsNotNull<Embed>(),
                        null,
                        null,
                        null));

                var (content, embed) = mockMessageLogChannel.Invocations
                    .Where(x => x.Method.Name == nameof(IMessageChannel.SendMessageAsync))
                    .Select(x => ((string)x.Arguments[0], (Embed)x.Arguments[2]))
                    .First();


                content.ShouldBe(string.Empty);

                embed.Description.ShouldContain("deleted", Case.Insensitive);
                embed.Fields.ShouldContain(x => x.Value == notification.Channel.Id.ToString());

                if (notification.Message.HasValue)
                {
                    embed.Author.HasValue.ShouldBeTrue();
                    embed.Fields.ShouldContain(x => x.Value == notification.Message.Value.Content.TruncateTo(EmbedFieldBuilder.MaxFieldValueLength));
                    if (notification.Message.Value.Attachments.Any())
                    {
                        embed.Fields.ShouldContain(x => x.Name == "Attachments");
                        var field = embed.Fields.First(x => x.Name == "Attachments");

                        foreach (var attachment in notification.Message.Value.Attachments)
                        {
                            field.Value.ShouldContain(attachment.Filename);
                            field.Value.ShouldContain(attachment.Size.ToString());
                        }
                    }
                }
                else
                    embed.Fields.ShouldContain(x => x.Value == "[N/A]");

                embed.Timestamp.ShouldNotBeNull();
            }

            mockChannel.ShouldNotHaveReceived(x => x
                .SendMessageAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<Embed?>(),
                    It.IsAny<RequestOptions?>(),
                    It.IsAny<AllowedMentions?>(),
                    It.IsAny<MessageReference?>()));
        }

        #endregion HandleNotificationAsync(MessageDeletedNotification) Tests

        #region HandleNotificationAsync(MessageUpdatedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            ulong authorId,
            ulong messageId,
            string? oldContent,
            string newContent,
            ulong channelId,
            ulong? guildId)
        {
            var mockAuthor = new Mock<IUser>();
            mockAuthor
                .Setup(x => x.Id)
                .Returns(authorId);

            var mockCacheableOldMessage = new Mock<ICacheable<IMessage, ulong>>();
            mockCacheableOldMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockCacheableOldMessage
                .Setup(x => x.HasValue)
                .Returns(oldContent is { });
            if (oldContent is { })
            {
                var mockOldMessage = new Mock<IMessage>();
                mockOldMessage
                    .Setup(x => x.Id)
                    .Returns(messageId);
                mockOldMessage
                    .Setup(x => x.Author)
                    .Returns(mockAuthor.Object);
                mockOldMessage
                    .Setup(x => x.Content)
                    .Returns(oldContent);

                mockCacheableOldMessage
                    .Setup(x => x.Value)
                    .Returns(mockOldMessage.Object);
            }

            var mockChannel = new Mock<IISocketMessageChannel>();
            mockChannel
                .Setup(x => x.Id)
                .Returns(channelId);
            if (guildId is null)
                mockChannel.As<IDMChannel>();
            else
                mockChannel.As<ITextChannel>()
                    .Setup(x => x.GuildId)
                    .Returns(guildId.Value);

            var mockNewMessage = new Mock<ISocketMessage>();
            mockNewMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockNewMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);
            mockNewMessage
                .Setup(x => x.Content)
                .Returns(newContent);
            mockNewMessage
                .Setup(x => x.Channel)
                .Returns(() => mockChannel.Object);

            Mock<ISocketGuild>? mockGuild = null;
            if (guildId.HasValue)
            {
                mockGuild = new Mock<ISocketGuild>();
                mockGuild
                    .Setup(x => x.Id)
                    .Returns(guildId.Value);

                mockChannel.As<ISocketGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);
            }

            var notification = new MessageUpdatedNotification(
                mockCacheableOldMessage.Object,
                mockNewMessage.Object,
                mockChannel.Object);

            return new TestCaseData(selfUserId, isChannelUnmoderated, messageLogChannelIds, notification, mockChannel, mockGuild);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageUpdatedNotification_MessageShouldBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: default,           isChannelUnmoderated: default,  messageLogChannelIds: Array.Empty<ulong>(),     authorId: default,          messageId: default,         oldContent: default,        newContent: string.Empty,   channelId: default,         guildId: default        ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: ulong.MinValue,    isChannelUnmoderated: false,    messageLogChannelIds: new[] { ulong.MinValue }, authorId: ulong.MinValue,   messageId: ulong.MinValue,  oldContent: string.Empty,   newContent: string.Empty,   channelId: ulong.MinValue,  guildId: ulong.MinValue ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: ulong.MaxValue,    isChannelUnmoderated: true,     messageLogChannelIds: new[] { ulong.MaxValue }, authorId: ulong.MaxValue,   messageId: ulong.MaxValue,  oldContent: string.Empty,   newContent: string.Empty,   channelId: ulong.MaxValue,  guildId: ulong.MaxValue ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 1UL,               isChannelUnmoderated: false,    messageLogChannelIds: new[] { 2UL },            authorId: 3UL,              messageId: 4UL,             oldContent: "5",            newContent: "6",            channelId: 7UL,             guildId: null           ).SetName("{m}(Message is not from Guild)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 8UL,               isChannelUnmoderated: false,    messageLogChannelIds: new[] { 9UL },            authorId: 8UL,              messageId: 10UL,            oldContent: "11",           newContent: "12",           channelId: 13UL,            guildId: 14UL           ).SetName("{m}(Message is from Self)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 15UL,              isChannelUnmoderated: true,     messageLogChannelIds: new[] { 16UL },           authorId: 17UL,             messageId: 18UL,            oldContent: "19",           newContent: "19",           channelId: 20UL,            guildId: 21UL           ).SetName("{m}(Message content has not changed)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 22UL,              isChannelUnmoderated: true,     messageLogChannelIds: new[] { 23UL },           authorId: 24UL,             messageId: 25UL,            oldContent: "26",           newContent: "27",           channelId: 28UL,            guildId: 29UL           ).SetName("{m}(Channel is Unmoderated)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 30UL,              isChannelUnmoderated: false,    messageLogChannelIds: Array.Empty<ulong>(),     authorId: 31UL,             messageId: 32UL,            oldContent: "33",           newContent: "34",           channelId: 35UL,            guildId: 36UL           ).SetName("{m}(No MessageLog channels designated)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageUpdatedNotification_MessageShouldBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageShouldBeIngored_IgnoresMessage(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            MessageUpdatedNotification notification,
            Mock<IISocketMessageChannel> mockChannel,
            Mock<ISocketGuild>? mockGuild)
        {
            using var testContext = new TestContext(
                selfUserId,
                isChannelUnmoderated,
                messageLogChannelIds);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(notification, testContext.CancellationToken);

            if (mockGuild is null)
                testContext.MockDesignatedChannelService.Invocations.ShouldBeEmpty();
            else
            {
                testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                        .ChannelHasDesignationAsync(
                            mockGuild.Object.Id,
                            mockChannel.Object.Id,
                            DesignatedChannelType.Unmoderated,
                            testContext.CancellationToken),
                    Times.AtMostOnce());
                testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                        .GetDesignatedChannelsAsync(
                            mockGuild.Object,
                            DesignatedChannelType.MessageLog),
                    Times.AtMostOnce());
            }

            foreach(var mockMessageLogChannel in testContext.MockMessageLogChannels)
                mockMessageLogChannel.Invocations.ShouldBeEmpty();

            mockChannel.ShouldNotHaveReceived(x => x
                .SendMessageAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<Embed?>(),
                    It.IsAny<RequestOptions?>(),
                    It.IsAny<AllowedMentions?>(),
                    It.IsAny<MessageReference?>()));
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageUpdatedNotification_MessageShouldNotBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 1UL,   isChannelUnmoderated: false,    messageLogChannelIds: new[] { 2UL },                authorId: 3UL,  messageId: 4UL,     oldContent: "5",                    newContent: "6",                    channelId: 7UL,     guildId: 8UL    ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 9UL,   isChannelUnmoderated: false,    messageLogChannelIds: new[] { 10UL, 11UL },         authorId: 12UL, messageId: 13UL,    oldContent: "14",                   newContent: "15",                   channelId: 16UL,    guildId: 17UL   ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 18UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 19UL, 20UL, 21UL },   authorId: 22UL, messageId: 23UL,    oldContent: "24",                   newContent: "25",                   channelId: 26UL,    guildId: 27UL   ).SetName("{m}(Unique Values 3)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 28UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 29UL },               authorId: 30UL, messageId: 31UL,    oldContent: null,                   newContent: "32",                   channelId: 33UL,    guildId: 34UL   ).SetName("{m}(OldMessage is not cached)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 35UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 36UL },               authorId: 37UL, messageId: 38UL,    oldContent: new string('A', 2000),  newContent: "39",                   channelId: 40UL,    guildId: 41UL   ).SetName("{m}(OldMessage content is max length)"),
                BuildTestCaseData_HandleNotificationAsync_MessageUpdatedNotification(selfUserId: 42UL,  isChannelUnmoderated: false,    messageLogChannelIds: new[] { 43UL },               authorId: 44UL, messageId: 45UL,    oldContent: "46",                   newContent: new string('B', 2000),  channelId: 47UL,    guildId: 48UL   ).SetName("{m}(NewMessage content is max length)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageUpdatedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageShouldNotBeIngored_LogsUpdatedMessageToMessageLogChannels(
            ulong selfUserId,
            bool isChannelUnmoderated,
            IReadOnlyList<ulong> messageLogChannelIds,
            MessageUpdatedNotification notification,
            Mock<IISocketMessageChannel> mockChannel,
            Mock<ISocketGuild> mockGuild)
        {
            using var testContext = new TestContext(
                selfUserId,
                isChannelUnmoderated,
                messageLogChannelIds);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(notification, testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    mockGuild.Object.Id,
                    mockChannel.Object.Id,
                    DesignatedChannelType.Unmoderated,
                    testContext.CancellationToken));
            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .GetDesignatedChannelsAsync(
                    mockGuild.Object,
                    DesignatedChannelType.MessageLog));

            foreach (var mockMessageLogChannel in testContext.MockMessageLogChannels)
            {
                mockMessageLogChannel.ShouldHaveReceived(x => x
                    .SendMessageAsync(
                        It.IsNotNull<string>(),
                        false,
                        It.IsNotNull<Embed>(),
                        null,
                        null,
                        null));

                var (content, embed) = mockMessageLogChannel.Invocations
                    .Where(x => x.Method.Name == nameof(IMessageChannel.SendMessageAsync))
                    .Select(x => ((string)x.Arguments[0], (Embed)x.Arguments[2]))
                    .First();

                content.ShouldBe(string.Empty);

                embed.Author.HasValue.ShouldBeTrue();
                embed.Description.ShouldContain("edited", Case.Insensitive);
                embed.Fields.ShouldContain(x => x.Value == notification.Channel.Id.ToString());
                embed.Fields.ShouldContain(x => x.Value == notification.NewMessage.Id.ToString());
                embed.Fields.ShouldContain(x => x.Value == notification.NewMessage.Content.TruncateTo(EmbedFieldBuilder.MaxFieldValueLength));

                if (notification.OldMessage.HasValue)
                    embed.Fields.ShouldContain(x => x.Value == notification.OldMessage.Value.Content.TruncateTo(EmbedFieldBuilder.MaxFieldValueLength));
                else
                    embed.Fields.ShouldContain(x => x.Value == "[N/A]");

                embed.Timestamp.ShouldNotBeNull();
            }

            mockChannel.ShouldNotHaveReceived(x => x
                .SendMessageAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<Embed?>(),
                    It.IsAny<RequestOptions?>(),
                    It.IsAny<AllowedMentions?>(),
                    It.IsAny<MessageReference?>()));
        }

        #endregion HandleNotificationAsync(MessageUpdatedNotification) Tests
    }
}
