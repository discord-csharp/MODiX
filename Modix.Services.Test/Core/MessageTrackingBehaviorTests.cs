#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using StatsdClient;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Data.Repositories;
using Modix.Services.Core;

using Modix.Common.Test;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class MessageTrackingBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext(
                int? messageCommandArgPos)
            {
                MockCommandPrefixParser = new Mock<ICommandPrefixParser>();
                MockCommandPrefixParser
                    .Setup(x => x.TryFindCommandArgPosAsync(It.IsAny<IUserMessage>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(messageCommandArgPos);

                MockDogStatsd = new Mock<IDogStatsd>();
                MockDogStatsd
                    .Setup(x => x.StartTimer(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
                    .Returns(() => MockDogStatsdTimer.Object);

                MockDogStatsdTimer = new Mock<IDisposable>();

                MockMaintainTransaction = new Mock<IRepositoryTransaction>();

                MockMessageRepository = new Mock<IMessageRepository>();
                MockMessageRepository
                    .Setup(x => x.BeginMaintainTransactionAsync())
                    .ReturnsAsync(() => MockMaintainTransaction.Object);
            }

            public MessageTrackingBehavior BuildUut()
                => new MessageTrackingBehavior(
                    MockCommandPrefixParser.Object,
                    MockDogStatsd.Object,
                    LoggerFactory.CreateLogger<MessageTrackingBehavior>(),
                    MockMessageRepository.Object);

            public readonly Mock<ICommandPrefixParser> MockCommandPrefixParser;
            public readonly Mock<IDogStatsd> MockDogStatsd;
            public readonly Mock<IDisposable> MockDogStatsdTimer;
            public readonly Mock<IRepositoryTransaction> MockMaintainTransaction;
            public readonly Mock<IMessageRepository> MockMessageRepository;
        }

        #endregion Test Context

        #region HandleNotificationAsync(MessageDeletedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(
            ulong? guildId,
            ulong channelId,
            ulong messageId,
            (ulong authorId, string content, bool isUserMessage, int? commandArgPos, bool authorIsBot, bool authorIsWebhook)? message)
        {
            Mock<ISocketGuild>? mockGuild = null;
            if (guildId is { })
            {
                mockGuild = new Mock<ISocketGuild>();
                mockGuild
                    .Setup(x => x.Id)
                    .Returns(guildId.Value);
            }

            var mockChannel = new Mock<IISocketMessageChannel>();
            mockChannel
                .Setup(x => x.Id)
                .Returns(channelId);
            if (mockGuild is { })
                mockChannel.As<IGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);

            Mock<IMessage>? mockMessage = null;
            if (message.HasValue)
            {
                var mockAuthor = new Mock<IUser>();
                mockAuthor
                    .Setup(x => x.Id)
                    .Returns(message.Value.authorId);
                mockAuthor
                    .Setup(x => x.IsBot)
                    .Returns(message.Value.authorIsBot);
                mockAuthor
                    .Setup(x => x.IsWebhook)
                    .Returns(message.Value.authorIsWebhook);

                mockMessage = new Mock<IMessage>();
                mockMessage
                    .Setup(x => x.Id)
                    .Returns(messageId);
                mockMessage
                    .Setup(x => x.Author)
                    .Returns(mockAuthor.Object);
                mockMessage
                    .Setup(x => x.Content)
                    .Returns(message.Value.content);
                if (message.Value.isUserMessage)
                    mockMessage.As<IUserMessage>();
            }

            var mockCacheableMessage = new Mock<ICacheable<IMessage, ulong>>();
            mockCacheableMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockCacheableMessage
                .Setup(x => x.HasValue)
                .Returns(message.HasValue);
            if (mockMessage is { })
                mockCacheableMessage
                    .Setup(x => x.Value)
                    .Returns(() => mockMessage.Object);
            else
                mockCacheableMessage
                    .Setup(x => x.Value)
                    .Throws<InvalidOperationException>();

            var notification = new MessageDeletedNotification(
                mockCacheableMessage.Object,
                mockChannel.Object);

            return new TestCaseData(
                message?.commandArgPos,
                notification,
                mockMessage);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: default,           channelId: default,         messageId: default,         message: default                                                                                                                                                ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: ulong.MinValue,    channelId: ulong.MinValue,  messageId: ulong.MinValue,  message: (authorId: ulong.MinValue, content: string.Empty,  isUserMessage: false,   commandArgPos: int.MinValue,    authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: ulong.MaxValue,    channelId: ulong.MaxValue,  messageId: ulong.MaxValue,  message: (authorId: ulong.MaxValue, content: string.Empty,  isUserMessage: true,    commandArgPos: int.MaxValue,    authorIsBot: true,  authorIsWebhook: true)  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: null,              channelId: 1UL,             messageId: 2UL,             message: (authorId: 3UL,            content: "4",           isUserMessage: true,    commandArgPos: -1,              authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Message is not from Guild)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 5UL,               channelId: 6UL,             messageId: 7UL,             message: (authorId: 8UL,            content: "9",           isUserMessage: false,   commandArgPos: -1,              authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Message is not from User)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 10UL,              channelId: 11UL,            messageId: 12UL,            message: (authorId: 13UL,           content: "14",          isUserMessage: true,    commandArgPos: -1,              authorIsBot: true,  authorIsWebhook: false) ).SetName("{m}(Message is from Bot)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 15UL,              channelId: 16UL,            messageId: 17UL,            message: (authorId: 18UL,           content: "19",          isUserMessage: true,    commandArgPos: -1,              authorIsBot: false, authorIsWebhook: true)  ).SetName("{m}(Message is from Webhook)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 20UL,              channelId: 21UL,            messageId: 22UL,            message: (authorId: 23UL,           content: "24",          isUserMessage: true,    commandArgPos: 25,              authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Message is command)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageDeletedNotification_MessageShouldBeIgnored_IgnoresMessage(
            int? messageCommandArgPos,
            MessageDeletedNotification notification,
            Mock<IMessage>? mockMessage)
        {
            using var testContext = new TestContext(
                messageCommandArgPos);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            if (mockMessage?.Object is IUserMessage userMessage)
                testContext.MockCommandPrefixParser.ShouldHaveReceived(x => x
                        .TryFindCommandArgPosAsync(userMessage, testContext.CancellationToken),
                    Times.AtMostOnce());
            else
                testContext.MockCommandPrefixParser.Invocations.ShouldBeEmpty();

            testContext.MockMessageRepository.Invocations.ShouldBeEmpty();
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 1UL,   channelId: 2UL,     messageId: 3UL,     message: (authorId: 4UL,    content: "5",   isUserMessage: true,    commandArgPos: null,    authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 6UL,   channelId: 7UL,     messageId: 8UL,     message: (authorId: 9UL,    content: "10",  isUserMessage: true,    commandArgPos: null,    authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 11UL,  channelId: 12UL,    messageId: 13UL,    message: (authorId: 14UL,   content: "15",  isUserMessage: true,    commandArgPos: null,    authorIsBot: false, authorIsWebhook: false) ).SetName("{m}(Unique Values 3)"),
                BuildTestCaseData_HandleNotificationAsync_MessageDeletedNotification(   guildId: 16UL,  channelId: 17UL,    messageId: 18UL,    message: null                                                                                                                           ).SetName("{m}(Message is not cached)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageDeletedNotification_MessageShouldNotBeIgnored_TracksMessage(
            int? messageCommandArgPos,
            MessageDeletedNotification notification,
            Mock<IMessage>? mockMessage)
        {
            using var testContext = new TestContext(
                messageCommandArgPos);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            if (mockMessage is { })
                testContext.MockCommandPrefixParser.ShouldHaveReceived(x => x
                    .TryFindCommandArgPosAsync((IUserMessage)mockMessage.Object, testContext.CancellationToken));
            else
                testContext.MockCommandPrefixParser.Invocations.ShouldBeEmpty();

            testContext.MockMessageRepository.ShouldHaveReceived(x => x
                .BeginMaintainTransactionAsync());
            testContext.MockMessageRepository.ShouldHaveReceived(x => x
                .DeleteAsync(notification.Message.Id));

            testContext.MockMaintainTransaction.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion HandleNotificationAsync(MessageDeletedNotification) Tests

        #region HandleNotificationAsync(MessageReceivedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(
            ulong? guildId,
            ulong channelId,
            ulong messageId,
            bool messageIsUserMessage,
            int? messageCommandArgPos,
            string messageContent,
            DateTimeOffset messageTimestamp,
            ulong authorId,
            bool authorIsBot,
            bool authorIsWebhook)
        {
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

                mockChannel.As<IGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);
                mockChannel.As<IGuildChannel>()
                    .Setup(x => x.GuildId)
                    .Returns(guildId.Value);
            }

            var mockAuthor = new Mock<IUser>();
            mockAuthor
                .Setup(x => x.Id)
                .Returns(authorId);
            mockAuthor
                .Setup(x => x.IsBot)
                .Returns(authorIsBot);
            mockAuthor
                .Setup(x => x.IsWebhook)
                .Returns(authorIsWebhook);

            var mockMessage = new Mock<ISocketMessage>();
            mockMessage
                .Setup(x => x.Id)
                .Returns(messageId);
            mockMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);
            mockMessage
                .Setup(x => x.Content)
                .Returns(messageContent);
            mockMessage
                .Setup(x => x.Timestamp)
                .Returns(messageTimestamp);
            mockMessage
                .Setup(x => x.Channel)
                .Returns(mockChannel.Object);
            if (messageIsUserMessage)
                mockMessage.As<IUserMessage>();

            var notification = new MessageReceivedNotification(
                mockMessage.Object);

            return new TestCaseData(
                messageCommandArgPos,
                notification,
                mockMessage);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: default,           channelId: default,         messageId: default,         messageIsUserMessage: default,  messageCommandArgPos: default,      messageContent: string.Empty,   messageTimestamp: default,                              authorId: default,          authorIsBot: default,   authorIsWebhook: default    ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: ulong.MinValue,    channelId: ulong.MinValue,  messageId: ulong.MinValue,  messageIsUserMessage: false,    messageCommandArgPos: int.MinValue, messageContent: string.Empty,   messageTimestamp: DateTimeOffset.MinValue,              authorId: ulong.MinValue,   authorIsBot: false,     authorIsWebhook: false      ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: ulong.MaxValue,    channelId: ulong.MaxValue,  messageId: ulong.MaxValue,  messageIsUserMessage: true,     messageCommandArgPos: int.MaxValue, messageContent: string.Empty,   messageTimestamp: DateTimeOffset.MaxValue,              authorId: ulong.MaxValue,   authorIsBot: true,      authorIsWebhook: true       ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: null,              channelId: 1UL,             messageId: 2UL,             messageIsUserMessage: true,     messageCommandArgPos: null,         messageContent: "3",            messageTimestamp: DateTimeOffset.Parse("2004-05-06"),   authorId: 7UL,              authorIsBot: false,     authorIsWebhook: false      ).SetName("{m}(Message is not from Guild)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 8UL,               channelId: 9UL,             messageId: 10UL,            messageIsUserMessage: false,    messageCommandArgPos: null,         messageContent: "11",           messageTimestamp: DateTimeOffset.Parse("2012-01-14"),   authorId: 15UL,             authorIsBot: false,     authorIsWebhook: false      ).SetName("{m}(Message is not from User)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 16UL,              channelId: 17UL,            messageId: 18UL,            messageIsUserMessage: true,     messageCommandArgPos: null,         messageContent: "19",           messageTimestamp: DateTimeOffset.Parse("2020-09-22"),   authorId: 23UL,             authorIsBot: true,      authorIsWebhook: false      ).SetName("{m}(Message is from Bot)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 24UL,              channelId: 25UL,            messageId: 26UL,            messageIsUserMessage: true,     messageCommandArgPos: null,         messageContent: "27",           messageTimestamp: DateTimeOffset.Parse("2028-05-30"),   authorId: 31UL,             authorIsBot: false,     authorIsWebhook: true       ).SetName("{m}(Message is from Webhook)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 32UL,              channelId: 33UL,            messageId: 34UL,            messageIsUserMessage: true,     messageCommandArgPos: 35,           messageContent: "36",           messageTimestamp: DateTimeOffset.Parse("2037-02-09"),   authorId: 40UL,             authorIsBot: false,     authorIsWebhook: true       ).SetName("{m}(Message is command)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_IgnoresMessage(
            int? messageCommandArgPos,
            MessageReceivedNotification notification,
            Mock<ISocketMessage> mockMessage)
        {
            using var testContext = new TestContext(
                messageCommandArgPos);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .StartTimer("message_processing_ms", 1, null));

            if (mockMessage.Object is IUserMessage userMessage)
                testContext.MockCommandPrefixParser.ShouldHaveReceived(x => x
                        .TryFindCommandArgPosAsync(userMessage, testContext.CancellationToken),
                    Times.AtMostOnce());
            else
                testContext.MockCommandPrefixParser.Invocations.ShouldBeEmpty();

            testContext.MockMessageRepository.Invocations.ShouldBeEmpty();

            testContext.MockDogStatsdTimer.ShouldHaveReceived(x => x                .Dispose());
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 1UL,   channelId: 2UL,     messageId: 3UL,     messageIsUserMessage: true, messageCommandArgPos: null, messageContent: "4",    messageTimestamp: DateTimeOffset.Parse("2005-06-07"),   authorId: 8UL,  authorIsBot: false, authorIsWebhook: false  ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 9UL,   channelId: 10UL,    messageId: 11UL,    messageIsUserMessage: true, messageCommandArgPos: null, messageContent: "12",   messageTimestamp: DateTimeOffset.Parse("2013-02-15"),   authorId: 16UL, authorIsBot: false, authorIsWebhook: false  ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guildId: 17UL,  channelId: 18UL,    messageId: 19UL,    messageIsUserMessage: true, messageCommandArgPos: null, messageContent: "20",   messageTimestamp: DateTimeOffset.Parse("2021-10-23"),   authorId: 24UL, authorIsBot: false, authorIsWebhook: false  ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TracksMessage(
            int? messageCommandArgPos,
            MessageReceivedNotification notification,
            Mock<ISocketMessage> mockMessage)
        {
            using var testContext = new TestContext(
                messageCommandArgPos);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .StartTimer("message_processing_ms", 1, null));

            testContext.MockCommandPrefixParser.ShouldHaveReceived(x => x
                .TryFindCommandArgPosAsync((IUserMessage)mockMessage.Object, testContext.CancellationToken));

            testContext.MockMessageRepository.ShouldHaveReceived(x => x
                .BeginMaintainTransactionAsync());
            testContext.MockMessageRepository.ShouldHaveReceived(x => x
                .CreateAsync(It.IsNotNull<MessageCreationData>()));

            var messageCreationData = testContext.MockMessageRepository.Invocations
                .Where(x => x.Method.Name == nameof(IMessageRepository.CreateAsync))
                .Select(x => (MessageCreationData)x.Arguments[0])
                .First();
            messageCreationData.Id.ShouldBe(mockMessage.Object.Id);
            messageCreationData.GuildId.ShouldBe(((IGuildChannel)mockMessage.Object.Channel).GuildId);
            messageCreationData.ChannelId.ShouldBe(mockMessage.Object.Channel.Id);
            messageCreationData.AuthorId.ShouldBe(mockMessage.Object.Author.Id);
            messageCreationData.Timestamp.ShouldBe(mockMessage.Object.Timestamp);

            testContext.MockMaintainTransaction.ShouldHaveReceived(x => x
                .Dispose());

            testContext.MockDogStatsdTimer.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion HandleNotificationAsync(MessageDeletedNotification) Tests
    }
}
