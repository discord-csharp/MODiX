using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.MessageContentPatterns;
using Modix.Services.Moderation;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

namespace Modix.Services.Test.Moderation
{
    [TestFixture]
    public class MessageContentCheckBehaviourTests
    {
        #region Test Context

        private static (AutoMocker autoMocker, MessageContentCheckBehaviour uut) BuildTestContext()
        {
            var autoMocker = new AutoMocker();

            var uut = autoMocker.CreateInstance<MessageContentCheckBehaviour>();

            var mockSelfUser = autoMocker.GetMock<ISocketSelfUser>();
            mockSelfUser
                .Setup(x => x.Id)
                .Returns(1);

            autoMocker.GetMock<IDiscordSocketClient>()
                .Setup(x => x.CurrentUser)
                .Returns(() => mockSelfUser.Object);

            autoMocker.GetMock<IDiscordSocketClient>()
                .Setup(x => x.GetInviteAsync(It.IsIn(GuildInviteCodes), It.IsAny<RequestOptions>()))
                .ReturnsAsync((string code, RequestOptions _) =>
                {
                    var mockInvite = autoMocker.GetMock<IRestInviteMetadata>();

                    mockInvite
                        .SetupGet(x => x.Code)
                        .Returns(code);

                    mockInvite
                        .SetupGet(x => x.GuildId)
                        .Returns(42);

                    return mockInvite.Object;
                });

            autoMocker.GetMock<IDiscordSocketClient>()
                .Setup(x => x.GetInviteAsync(It.IsNotIn(GuildInviteCodes), It.IsAny<RequestOptions>()))
                .ReturnsAsync((string code, RequestOptions _) =>
                {
                    var mockInvite = autoMocker.GetMock<IRestInviteMetadata>();

                    mockInvite
                        .SetupGet(x => x.Code)
                        .Returns(code);

                    mockInvite
                        .SetupGet(x => x.GuildId)
                        .Returns(77);

                    return mockInvite.Object;
                });

            var mockMessageContentPatternService = autoMocker.GetMock<IMessageContentPatternService>();
            mockMessageContentPatternService
                .Setup(x => x.GetPatterns(It.IsAny<ulong>()))
                .ReturnsAsync(new List<MessageContentPatternDto>());

            return (autoMocker, uut);
        }

        private static ISocketMessage BuildRandomContentTestMessage(AutoMocker autoMocker)
        {
            var mockMessage = autoMocker.GetMock<ISocketMessage>();
            var mockGuild = autoMocker.GetMock<IGuild>();

            var mockAuthor = autoMocker.GetMock<IGuildUser>();
            mockAuthor
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockAuthor
                .Setup(x => x.GuildId)
                .Returns(42);
            mockAuthor
                .Setup(x => x.Id)
                .Returns(2);
            mockAuthor
                .Setup(x => x.Mention)
                .Returns("<@2>");
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
                .Returns("Hello world");

            var patternServiceMock = autoMocker.GetMock<IMessageContentPatternService>();
            patternServiceMock.Setup(x => x.GetPatterns(It.IsAny<ulong>()))
                .ReturnsAsync(new List<MessageContentPatternDto>
                {
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)",
                        MessageContentPatternType.Blocked)
                });

            return mockMessage.Object;
        }

        private static ISocketMessage BuildBlockedContentTestMessage(AutoMocker autoMocker)
        {
            var mockMessage = autoMocker.GetMock<ISocketMessage>();
            var mockGuild = autoMocker.GetMock<IGuild>();

            var mockAuthor = autoMocker.GetMock<IGuildUser>();
            mockAuthor
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockAuthor
                .Setup(x => x.GuildId)
                .Returns(42);
            mockAuthor
                .Setup(x => x.Id)
                .Returns(2);
            mockAuthor
                .Setup(x => x.Mention)
                .Returns("<@2>");
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
                .Returns(BlockedContent.url);

            var patternServiceMock = autoMocker.GetMock<IMessageContentPatternService>();
            patternServiceMock.Setup(x => x.GetPatterns(It.IsAny<ulong>()))
                .ReturnsAsync(new List<MessageContentPatternDto>
                {
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)",
                        MessageContentPatternType.Blocked)
                });

            return mockMessage.Object;
        }

        private static ISocketMessage BuildBlockedAndAllowedContentTestMessage(AutoMocker autoMocker)
        {
            var mockMessage = autoMocker.GetMock<ISocketMessage>();
            var mockGuild = autoMocker.GetMock<IGuild>();

            var mockAuthor = autoMocker.GetMock<IGuildUser>();
            mockAuthor
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockAuthor
                .Setup(x => x.GuildId)
                .Returns(42);
            mockAuthor
                .Setup(x => x.Id)
                .Returns(2);
            mockAuthor
                .Setup(x => x.Mention)
                .Returns("<@2>");
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
                .Returns($"{BlockedContent.url} discord.gg/allowed");

            var patternServiceMock = autoMocker.GetMock<IMessageContentPatternService>();
            patternServiceMock.Setup(x => x.GetPatterns(It.IsAny<ulong>()))
                .ReturnsAsync(new List<MessageContentPatternDto>
                {
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)",
                        MessageContentPatternType.Blocked),
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(allowed)",
                        MessageContentPatternType.Allowed),
                });

            return mockMessage.Object;
        }

        private static ISocketMessage BuildBlockedButSupersededContentTestMessage(AutoMocker autoMocker)
        {
            var mockMessage = autoMocker.GetMock<ISocketMessage>();
            var mockGuild = autoMocker.GetMock<IGuild>();

            var mockAuthor = autoMocker.GetMock<IGuildUser>();
            mockAuthor
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);
            mockAuthor
                .Setup(x => x.GuildId)
                .Returns(42);
            mockAuthor
                .Setup(x => x.Id)
                .Returns(2);
            mockAuthor
                .Setup(x => x.Mention)
                .Returns("<@2>");
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
                .Returns("https://discord.gg/foobar");

            var patternServiceMock = autoMocker.GetMock<IMessageContentPatternService>();
            patternServiceMock.Setup(x => x.GetPatterns(It.IsAny<ulong>()))
                .ReturnsAsync(new List<MessageContentPatternDto>
                {
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)",
                        MessageContentPatternType.Blocked),
                    new MessageContentPatternDto(
                        "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/foobar)",
                        MessageContentPatternType.Allowed),
                });

            return mockMessage.Object;
        }

        #endregion Test Context

        #region Test Data

        private static readonly (string url, string pattern) BlockedContent
            = ("https://discord.gg/111111", "(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)");

        private static readonly string DefaultInviteLink
            = "https://discord.gg/111111";

        private static readonly string[] InviteLinkMessages
            = {
                "https://discord.gg/111111",
                "https://discord.gg/222222",
                "https://discord.gg/abcdef",
                "https://discord.io/111111",
                "https://discord.me/111111",
                "https://discord.li/111111",
                "https://www.discord.gg/111111",
                "http://discord.gg/111111",
                "https://discordapp.com/invite/111111",
                "https://discord.com/invite/111111",
                "discord.gg/111111",
                "Yo, check out this server https://discord.gg/111111, it's totally lit"
            };

        private static readonly string[] NonInviteLinkMessages
            = {
                "No invite link here",
                "https://teamspeak.gg/123456",
                "google.com",
                "google-plus.com/invites/123456"
            };

        private static readonly string[] GuildInviteLinks
            = {
                "https://discord.gg/123456",
                "https://discord.gg/234567",
                "https://discord.gg/345678",
            };

        private static readonly string[] GuildInviteCodes
            = {
                "123456",
                "234567",
                "345678",
            };

        #endregion Test Data

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageAuthorIsNotGuildUser_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<ISocketMessage>()
                .Setup(x => x.Author)
                .Returns(autoMocker.Get<IUser>());

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageAuthorGuildIsNull_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<IGuildUser>()
                .Setup(x => x.Guild)
                .Returns<IGuild>(null);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageChannelIsNotGuildChannel_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<ISocketMessage>()
                .Setup(x => x.Channel)
                .Returns(new Mock<IMessageChannel>().Object);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageChannelGuildIsNull_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<IMessageChannel>()
                .As<IGuildChannel>()
                .Setup(x => x.Guild)
                .Returns<IGuild>(null);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageAuthorIsSelfUser_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<ISocketSelfUser>()
                .Setup(x => x.Id)
                .Returns(autoMocker.Get<IGuildUser>()!.Id);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(NonInviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageDoesNotContainBlockedContent_DoesNotDeleteMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildRandomContentTestMessage(autoMocker));

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageChannelIsUnmoderated_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            autoMocker.GetMock<IDesignatedChannelService>()
                .Setup(x => x.ChannelHasDesignationAsync(
                    autoMocker.Get<IGuild>().Id,
                    autoMocker.Get<IMessageChannel>().Id,
                    DesignatedChannelType.Unmoderated,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageAuthorHasByPassMessagePatternCheck_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            var user = autoMocker.Get<IGuildUser>();
            var roles = user.RoleIds?.ToList();
            autoMocker.GetMock<IAuthorizationService>()
                .Setup(x => x.HasClaimsAsync(
                    user.Id,
                    user.Guild.Id,
                    roles,
                    AuthorizationClaim.BypassMessageContentPatternCheck))
                .ReturnsAsync(true);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(GuildInviteLinks))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_ContentIsSupersededByAllow_DoesNotDeleteMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedButSupersededContentTestMessage(autoMocker));

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(InviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_OtherwiseContentNotBlocked_DoesNotDeletesMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildRandomContentTestMessage(autoMocker));

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(InviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_OtherwiseContentBlocked_DeletesMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedContentTestMessage(autoMocker));

            await uut.HandleNotificationAsync(notification);

            autoMocker.GetMock<IModerationService>()
                .ShouldHaveReceived(x => x.
                    DeleteMessageAsync(
                        notification.Message,
                        It.Is<string>(y => y.Contains("Message Content", StringComparison.OrdinalIgnoreCase)),
                        autoMocker.Get<ISocketSelfUser>().Id,
                        It.IsAny<CancellationToken>()));
        }

        [TestCaseSource(nameof(InviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_ContainsBlockedAndAllowedContent_DeletesMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageReceivedNotification(
                message: BuildBlockedAndAllowedContentTestMessage(autoMocker));

            await uut.HandleNotificationAsync(notification);

            autoMocker.GetMock<IModerationService>()
                .ShouldHaveReceived(x => x.
                    DeleteMessageAsync(
                        notification.Message,
                        It.Is<string>(y => y.Contains("Message Content", StringComparison.OrdinalIgnoreCase)),
                        autoMocker.Get<ISocketSelfUser>().Id,
                        It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageAuthorIsNotGuildUser_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<ISocketMessage>()
                .Setup(x => x.Author)
                .Returns(autoMocker.Get<IUser>());

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageAuthorGuildIsNull_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<IGuildUser>()
                .Setup(x => x.Guild)
                .Returns<IGuild>(null);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageChannelIsNotGuildChannel_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<ISocketMessage>()
                .Setup(x => x.Channel)
                .Returns(new Mock<IMessageChannel>().Object);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageChannelGuildIsNull_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<IMessageChannel>()
                .As<IGuildChannel>()
                .Setup(x => x.Guild)
                .Returns<IGuild>(null);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageAuthorIsSelfUser_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<ISocketSelfUser>()
                .Setup(x => x.Id)
                .Returns(autoMocker.Get<IGuildUser>()!.Id);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(NonInviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageDoesNotContainBlockedPattern_DoesNotDeleteMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildRandomContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageChannelIsUnmoderated_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            autoMocker.GetMock<IDesignatedChannelService>()
                .Setup(x => x.ChannelHasDesignationAsync(
                    autoMocker.Get<IGuild>().Id,
                    autoMocker.Get<IMessageChannel>().Id,
                    DesignatedChannelType.Unmoderated,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [Test]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_MessageAuthorHasBypassMessagePatternCheck_DoesNotDeleteMessage()
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());
            var user = autoMocker.Get<IGuildUser>();
            var roles = user.RoleIds?.ToList();
            autoMocker.GetMock<IAuthorizationService>()
                .Setup(x => x.HasClaimsAsync(
                    user.Id,
                    user.Guild.Id,
                    roles,
                    AuthorizationClaim.BypassMessageContentPatternCheck))
                .ReturnsAsync(true);

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(GuildInviteLinks))]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_ContentIsSupersededByAllow_DoesNotDeleteMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedButSupersededContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            await uut.HandleNotificationAsync(notification);

            autoMocker.MessageShouldNotHaveBeenDeleted();
        }

        [TestCaseSource(nameof(InviteLinkMessages))]
        public async Task HandleNotificationAsync_MessageUpdatedNotification_Otherwise_DeletesMessage(string messageContent)
        {
            (var autoMocker, var uut) = BuildTestContext();

            var notification = new MessageUpdatedNotification(
                oldMessage: autoMocker.Get<ICacheable<IMessage, ulong>>(),
                newMessage: BuildBlockedContentTestMessage(autoMocker),
                channel: autoMocker.Get<IISocketMessageChannel>());

            await uut.HandleNotificationAsync(notification);

            autoMocker.GetMock<IModerationService>()
                .ShouldHaveReceived(x => x.
                    DeleteMessageAsync(
                        notification.NewMessage,
                        It.Is<string>(y => y.Contains("Message Content", StringComparison.OrdinalIgnoreCase)),
                        autoMocker.Get<ISocketSelfUser>().Id,
                        It.IsAny<CancellationToken>()));
        }
    }

    internal static class MessageContentCheckBehaviorAssertions
    {
        public static void MessageShouldNotHaveBeenDeleted(this AutoMocker autoMocker)
        {
            autoMocker.GetMock<IModerationService>()
                .ShouldNotHaveReceived(x => x.
                    DeleteMessageAsync(
                        It.IsAny<IMessage>(),
                        It.IsAny<string>(),
                        It.IsAny<ulong>(),
                        It.IsAny<CancellationToken>()));

            autoMocker.GetMock<IMessageChannel>()
                .ShouldNotHaveReceived(x => x.
                    SendMessageAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<Embed>(),
                        It.IsAny<RequestOptions>(),
                        It.IsAny<AllowedMentions>(),
                        It.IsAny<MessageReference>()));
        }
    }
}
