#nullable enable

using System;
using System.Collections.Generic;
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

using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.UserMetrics;

using Modix.Common.Test;

namespace Modix.Services.Test.UserMetrics
{
    [TestFixture]
    public class UserMetricsBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext()
            {
                MockDesignatedChannelService = new Mock<IDesignatedChannelService>();
                MockDesignatedChannelService
                    .Setup(x => x.ChannelHasDesignationAsync(
                        It.IsAny<IGuild>(),
                        It.IsAny<IChannel>(),
                        DesignatedChannelType.CountsTowardsParticipation))
                    .ReturnsAsync(() => IsChannelOnTopic);

                MockDesignatedRoleService = new Mock<IDesignatedRoleService>();
                MockDesignatedRoleService
                    .Setup(x => x.RolesHaveDesignationAsync(
                        It.IsAny<ulong>(),
                        It.IsAny<IReadOnlyCollection<ulong>>(),
                        DesignatedRoleType.Rank,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => IsUserRanked);

                MockDogStatsd = new Mock<IDogStatsd>();
            }

            public UserMetricsBehavior BuildUut()
                => new UserMetricsBehavior(
                    MockDesignatedChannelService.Object,
                    MockDesignatedRoleService.Object,
                    MockDogStatsd.Object,
                    LoggerFactory.CreateLogger<UserMetricsBehavior>());

            public readonly Mock<IDesignatedChannelService> MockDesignatedChannelService;
            public readonly Mock<IDesignatedRoleService> MockDesignatedRoleService;
            public readonly Mock<IDogStatsd> MockDogStatsd;

            public bool IsChannelOnTopic;
            public bool IsUserRanked;
        }

        #endregion Test Context

        #region HandleNotificationAsync(GuildAvailableNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(
            int guildMemberCount,
            string guildName)
        {
            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.MemberCount)
                .Returns(guildMemberCount);
            mockGuild
                .Setup(x => x.Name)
                .Returns(guildName);

            var notification = new GuildAvailableNotification(
                mockGuild.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_GuildAvailableNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: default,      guildName: string.Empty ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: int.MinValue, guildName: string.Empty ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: int.MaxValue, guildName: string.Empty ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: 1,            guildName: "2"          ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: 3,            guildName: "4"          ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   guildMemberCount: 5,            guildName: "6"          ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_GuildAvailableNotification_TestCaseData))]
        public async Task HandleNotificationAsync_GuildAvailableNotification_Always_UpdatesStats(
            GuildAvailableNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Gauge(
                    UserMetricsBehavior.UserCountGaugeName,
                    notification.Guild.MemberCount,
                    1,
                    It.IsNotNull<string[]>()));

            var tags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Gauge))
                .Select(x => (string[])x.Arguments[3])
                .First();

            tags.ShouldContain(x => x.Contains(notification.Guild.Name));
        }

        #endregion HandleNotificationAsync(GuildAvailableNotification) Tests

        #region HandleNotificationAsync(MessageReceivedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(
            (ulong id, string name)?  guild,
            (ulong id, string name, bool isOnTopic) channel,
            (ulong id, bool isBot, bool isWebhook, bool isRanked, IReadOnlyCollection<ulong> roleIds) author,
            ulong messageId)
        {
            var mockChannel = new Mock<IMessageChannel>();
            mockChannel
                .Setup(x => x.Id)
                .Returns(channel.id);
            mockChannel
                .Setup(x => x.Name)
                .Returns(channel.name);

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

            var mockGuild = null as Mock<IGuild>;
            if (guild.HasValue)
            {
                mockGuild = new Mock<IGuild>();
                mockGuild
                    .Setup(x => x.Id)
                    .Returns(guild.Value.id);
                mockGuild
                    .Setup(x => x.Name)
                    .Returns(guild.Value.name);

                mockChannel.As<IGuildChannel>()
                    .Setup(x => x.Guild)
                    .Returns(mockGuild.Object);

                mockAuthor.As<IGuildUser>()
                    .Setup(x => x.GuildId)
                    .Returns(guild.Value.id);
                mockAuthor.As<IGuildUser>()
                    .Setup(x => x.RoleIds)
                    .Returns(author.roleIds);
            }

            var mockMessage = new Mock<ISocketMessage>();
            mockMessage
                .Setup(x => x.Channel)
                .Returns(mockChannel.Object);
            mockMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);

            var notification = new MessageReceivedNotification(mockMessage.Object);

            return new TestCaseData(notification, mockGuild?.Object, mockAuthor.Object, channel.isOnTopic, author.isRanked);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: ulong.MaxValue,    name: string.Empty),    channel:    (id: ulong.MaxValue,    name: string.Empty, isOnTopic: true),       author: (id: ulong.MaxValue,    isBot: true,    isWebhook: true,    isRanked: true,     roleIds: new[] { ulong.MaxValue }), messageId: ulong.MaxValue   ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  null,                                           channel:    (id: 1,                 name: "2",          isOnTopic: false),      author: (id: 3,                 isBot: false,   isWebhook: false,   isRanked: false,    roleIds: new[] { 4UL }),            messageId: 5                ).SetName("{m}(Message is not from Guild)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: 6,                 name: "7"),             channel:    (id: 8,                 name: "9",          isOnTopic: false),      author: (id: 10,                isBot: true,    isWebhook: false,   isRanked: false,    roleIds: new[] { 11UL }),           messageId: 12               ).SetName("{m}(Author is Bot)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: 13,                name: "14"),            channel:    (id: 15,                name: "16",         isOnTopic: false),      author: (id: 17,                isBot: false,   isWebhook: true,    isRanked: false,    roleIds: new[] { 18UL }),           messageId: 19               ).SetName("{m}(Author is Webhook)"));

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: 1,                 name: "2"),             channel:    (id: 3,                 name: "4",          isOnTopic: false),      author: (id: 5,                 isBot: false,   isWebhook: false,   isRanked: false,    roleIds: Array.Empty<ulong>()),     messageId: 6                ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: default,           name: string.Empty),    channel:    (id: default,           name: string.Empty, isOnTopic: default),    author: (id: default,           isBot: default, isWebhook: default, isRanked: default,  roleIds: new[] { default(ulong) }), messageId: default          ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: 7,                 name: "8"),             channel:    (id: 9,                 name: "10",         isOnTopic: true),       author: (id: 11,                isBot: false,   isWebhook: false,   isRanked: false,    roleIds: new[] { 12UL }),           messageId: 13               ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  guild:  (id: 14,                name: "15"),            channel:    (id: 16,                name: "17",         isOnTopic: false),      author: (id: 18,                isBot: false,   isWebhook: false,   isRanked: true,     roleIds: new[] { 19UL, 20UL }),     messageId: 21               ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageShouldBeIgnored_IgnoresMessage(
            MessageReceivedNotification notification,
            IGuild? guild,
            IUser author,
            bool isChannelOnTopic,
            bool isAuthorRanked)
        {
            var testContext = new TestContext()
            {
                IsChannelOnTopic = isChannelOnTopic,
                IsUserRanked = isAuthorRanked
            };

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                    .ChannelHasDesignationAsync(
                        guild,
                        notification.Message.Channel,
                        DesignatedChannelType.CountsTowardsParticipation),
                Times.AtMostOnce());

            if (guild is { })
                testContext.MockDesignatedRoleService.ShouldHaveReceived(x => x
                        .RolesHaveDesignationAsync(
                            guild.Id,
                            ((IGuildUser)author).RoleIds,
                            DesignatedRoleType.Rank,
                            testContext.CancellationToken),
                    Times.AtMostOnce());
            else
                testContext.MockDesignatedRoleService.ShouldNotHaveReceived(x => x
                    .RolesHaveDesignationAsync(
                        It.IsAny<ulong>(),
                        It.IsAny<IReadOnlyCollection<ulong>>(),
                        It.IsAny<DesignatedRoleType>(),
                        It.IsAny<CancellationToken>()));

            testContext.MockDogStatsd.Invocations.ShouldBeEmpty();
        }

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_UpdatesStats(
            MessageReceivedNotification notification,
            IGuild guild,
            IGuildUser author,
            bool isChannelOnTopic,
            bool isAuthorRanked)
        {
            var testContext = new TestContext()
            {
                IsChannelOnTopic = isChannelOnTopic,
                IsUserRanked = isAuthorRanked
            };

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    guild,
                    notification.Message.Channel,
                    DesignatedChannelType.CountsTowardsParticipation));

            testContext.MockDesignatedRoleService.ShouldHaveReceived(x => x
                .RolesHaveDesignationAsync(
                    guild.Id,
                    author.RoleIds,
                    DesignatedRoleType.Rank,
                    testContext.CancellationToken));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.MessageReceivedCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var tags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            tags.ShouldContain(x => x.Contains(notification.Message.Channel.Name));
            tags.ShouldContain(x => x.Contains(guild.Name));
            tags.ShouldContain(x => x.Contains((!isChannelOnTopic).ToString()));
            tags.ShouldContain(x => x.Contains(isAuthorRanked.ToString()));
        }

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_ChannelParticipationCheckThrowsException_UpdatesStats(
            MessageReceivedNotification notification,
            IGuild guild,
            IGuildUser author,
            bool isChannelOnTopic,
            bool isAuthorRanked)
        {
            var testContext = new TestContext()
            {
                IsChannelOnTopic = isChannelOnTopic,
                IsUserRanked = isAuthorRanked
            };

            testContext.MockDesignatedChannelService
                .Setup(x => x.ChannelHasDesignationAsync(
                    It.IsAny<IGuild>(),
                    It.IsAny<IChannel>(),
                    It.IsAny<DesignatedChannelType>()))
                .ThrowsAsync(new Exception());

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    guild,
                    notification.Message.Channel,
                    DesignatedChannelType.CountsTowardsParticipation));

            testContext.MockDesignatedRoleService.ShouldHaveReceived(x => x
                .RolesHaveDesignationAsync(
                    guild.Id,
                    author.RoleIds,
                    DesignatedRoleType.Rank,
                    testContext.CancellationToken));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.MessageReceivedCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var tags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            tags.ShouldContain(x => x.Contains(notification.Message.Channel.Name));
            tags.ShouldContain(x => x.Contains(guild.Name));
            tags.ShouldContain(x => x.Contains(isAuthorRanked.ToString()));
        }

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_MessageShouldNotBeIgnored_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_UserRankCheckThrowsException_UpdatesStats(
            MessageReceivedNotification notification,
            IGuild guild,
            IGuildUser author,
            bool isChannelOnTopic,
            bool isAuthorRanked)
        {
            var testContext = new TestContext()
            {
                IsChannelOnTopic = isChannelOnTopic,
                IsUserRanked = isAuthorRanked
            };

            testContext.MockDesignatedRoleService
                .Setup(x => x.RolesHaveDesignationAsync(
                    It.IsAny<ulong>(),
                    It.IsAny<IReadOnlyCollection<ulong>>(),
                    It.IsAny<DesignatedRoleType>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDesignatedChannelService.ShouldHaveReceived(x => x
                .ChannelHasDesignationAsync(
                    guild,
                    notification.Message.Channel,
                    DesignatedChannelType.CountsTowardsParticipation));

            testContext.MockDesignatedRoleService.ShouldHaveReceived(x => x
                .RolesHaveDesignationAsync(
                    guild.Id,
                    author.RoleIds,
                    DesignatedRoleType.Rank,
                    testContext.CancellationToken));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.MessageReceivedCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var tags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            tags.ShouldContain(x => x.Contains(notification.Message.Channel.Name));
            tags.ShouldContain(x => x.Contains(guild.Name));
            tags.ShouldContain(x => x.Contains((!isChannelOnTopic).ToString()));
        }

        #endregion HandleNotificationAsync(MessageReceivedNotification) Tests

        #region HandleNotificationAsync(UserBannedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(
            int guildMemberCount,
            string guildName)
        {
            var mockUser = new Mock<ISocketUser>();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.MemberCount)
                .Returns(guildMemberCount);
            mockGuild
                .Setup(x => x.Name)
                .Returns(guildName);

            var notification = new UserBannedNotification(
                mockUser.Object,
                mockGuild.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_UserBannedNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: default,      guildName: string.Empty ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: int.MinValue, guildName: string.Empty ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: int.MaxValue, guildName: string.Empty ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: 1,            guildName: "2"          ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: 3,            guildName: "4"          ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_UserBannedNotification(   guildMemberCount: 5,            guildName: "6"          ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_UserBannedNotification_TestCaseData))]
        public async Task HandleNotificationAsync_UserBannedNotification_Tests(
            UserBannedNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.UserBannedCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var incrementTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            incrementTags.ShouldContain(x => x.Contains(notification.Guild.Name));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Gauge(
                    UserMetricsBehavior.UserCountGaugeName,
                    notification.Guild.MemberCount,
                    1,
                    It.IsNotNull<string[]>()));

            var gaugeTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Gauge))
                .Select(x => (string[])x.Arguments[3])
                .First();

            gaugeTags.ShouldContain(x => x.Contains(notification.Guild.Name));
        }

        #endregion HandleNotificationAsync(UserBannedNotification) Tests

        #region HandleNotificationAsync(UserJoinedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(
            int guildMemberCount,
            string guildName)
        {
            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.MemberCount)
                .Returns(guildMemberCount);
            mockGuild
                .Setup(x => x.Name)
                .Returns(guildName);

            var mockGuildUser = new Mock<ISocketGuildUser>();
            mockGuildUser
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);

            var notification = new UserJoinedNotification(
                mockGuildUser.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_UserJoinedNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: default,      guildName: string.Empty ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: int.MinValue, guildName: string.Empty ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: int.MaxValue, guildName: string.Empty ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: 1,            guildName: "2"          ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: 3,            guildName: "4"          ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_UserJoinedNotification(   guildMemberCount: 5,            guildName: "6"          ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_UserJoinedNotification_TestCaseData))]
        public async Task HandleNotificationAsync_UserJoinedNotification_Tests(
            UserJoinedNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.UserJoinedCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var incrementTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            incrementTags.ShouldContain(x => x.Contains(notification.GuildUser.Guild.Name));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Gauge(
                    UserMetricsBehavior.UserCountGaugeName,
                    notification.GuildUser.Guild.MemberCount,
                    1,
                    It.IsNotNull<string[]>()));

            var gaugeTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Gauge))
                .Select(x => (string[])x.Arguments[3])
                .First();

            gaugeTags.ShouldContain(x => x.Contains(notification.GuildUser.Guild.Name));
        }

        #endregion HandleNotificationAsync(UserJoinedNotification) Tests

        #region HandleNotificationAsync(UserLeftNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_UserLeftNotification(
            int guildMemberCount,
            string guildName)
        {
            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.MemberCount)
                .Returns(guildMemberCount);
            mockGuild
                .Setup(x => x.Name)
                .Returns(guildName);

            var mockGuildUser = new Mock<ISocketGuildUser>();
            mockGuildUser
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);

            var notification = new UserLeftNotification(
                mockGuildUser.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_UserLeftNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: default,      guildName: string.Empty ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: int.MinValue, guildName: string.Empty ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: int.MaxValue, guildName: string.Empty ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: 1,            guildName: "2"          ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: 3,            guildName: "4"          ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_UserLeftNotification( guildMemberCount: 5,            guildName: "6"          ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_UserLeftNotification_TestCaseData))]
        public async Task HandleNotificationAsync_UserLeftNotification_Tests(
            UserLeftNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Increment(
                    UserMetricsBehavior.UserLeftCounterName,
                    1,
                    1,
                    It.IsNotNull<string[]>()));

            var incrementTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Increment))
                .Select(x => (string[])x.Arguments[3])
                .First();

            incrementTags.ShouldContain(x => x.Contains(notification.GuildUser.Guild.Name));

            testContext.MockDogStatsd.ShouldHaveReceived(x => x
                .Gauge(
                    UserMetricsBehavior.UserCountGaugeName,
                    notification.GuildUser.Guild.MemberCount,
                    1,
                    It.IsNotNull<string[]>()));

            var gaugeTags = testContext.MockDogStatsd.Invocations
                .Where(x => x.Method.Name == nameof(IDogStatsd.Gauge))
                .Select(x => (string[])x.Arguments[3])
                .First();

            gaugeTags.ShouldContain(x => x.Contains(notification.GuildUser.Guild.Name));
        }

        #endregion HandleNotificationAsync(UserLeftNotification) Tests
    }
}
