#nullable enable

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Services.Core;

using Modix.Common.Test;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class UserTrackingBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodTestContext
        {
            public TestContext()
            {
                MockSelfUser = new Mock<ISocketSelfUser>();
                MockSelfUser
                    .Setup(x => x.Id)
                    .Returns(() => SelfUserId);

                MockSelfUserProvider = new Mock<ISelfUserProvider>();
                MockSelfUserProvider
                    .Setup(x => x.GetSelfUserAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => MockSelfUser.Object);

                MockUserService = new Mock<IUserService>();
            }

            public UserTrackingBehavior BuildUut()
                => new UserTrackingBehavior(
                    MockSelfUserProvider.Object,
                    MockUserService.Object);

            public readonly Mock<ISocketSelfUser> MockSelfUser;
            public readonly Mock<ISelfUserProvider> MockSelfUserProvider;
            public readonly Mock<IUserService> MockUserService;

            public ulong SelfUserId;
        }

        #endregion Test Context

        #region HandleNotificationAsync(GuildAvailableNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(
            ulong selfUserId)
        {
            var mockGuildUser = new Mock<ISocketGuildUser>();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.GetUser(It.IsAny<ulong>()))
                .Returns(mockGuildUser.Object);

            var notification = new GuildAvailableNotification(
                mockGuild.Object);

            return new TestCaseData(selfUserId, mockGuild, mockGuildUser, notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_GuildAvailableNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: default         ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: ulong.MinValue  ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: ulong.MaxValue  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: 1               ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: 2               ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_GuildAvailableNotification(   selfUserId: 3               ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_GuildAvailableNotification_TestCaseData))]
        public async Task HandleNotificationAsync_GuildAvailableNotification_Always_TracksSelfUser(
            ulong selfUserId,
            Mock<ISocketGuild> mockGuild,
            Mock<ISocketGuildUser> mockGuildUser,
            GuildAvailableNotification notification)
        {
            var testContext = new TestContext()
            {
                SelfUserId = selfUserId
            };

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            mockGuild.ShouldHaveReceived(x => x
                .GetUser(selfUserId));

            testContext.MockUserService.ShouldHaveReceived(x => x
                .TrackUserAsync(mockGuildUser.Object, testContext.CancellationToken));
        }

        #endregion HandleNotificationAsync(GuildAvailableNotification) Tests

        #region HandleNotificationAsync(GuildMemberUpdatedNotification) Tests

        [Test]
        public async Task HandleNotificationAsync_GuildMemberUpdatedNotification_Always_TracksNewMember()
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            var notification = new GuildMemberUpdatedNotification(
                new Mock<ISocketGuildUser>().Object,
                new Mock<ISocketGuildUser>().Object);

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockUserService.ShouldHaveReceived(x => x
                .TrackUserAsync(notification.NewMember, testContext.CancellationToken));
        }

        #endregion HandleNotificationAsync(GuildMemberUpdatedNotification) Tests

        #region HandleNotificationAsync(MessageReceivedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(
            bool authorIsGuildMember,
            bool authorGuildIsNull)
        {
            var mockAuthor = new Mock<IUser>();
            if (authorIsGuildMember)
                mockAuthor.As<IGuildUser>()
                    .Setup(x => x.Guild)
                    .Returns(authorGuildIsNull
                        ? null!
                        : new Mock<IGuild>().Object);

            var mockMessage = new Mock<ISocketMessage>();
            mockMessage
                .Setup(x => x.Author)
                .Returns(mockAuthor.Object);

            var notification = new MessageReceivedNotification(
                mockMessage.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_AuthorIsNotTrackable_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  authorIsGuildMember: false, authorGuildIsNull: false).SetName("{m}(Author is not guild member)"),
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  authorIsGuildMember: true,  authorGuildIsNull: true ).SetName("{m}(Author guild is null)"));

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_MessageReceivedNotification_AuthorIsTrackable_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_MessageReceivedNotification(  authorIsGuildMember: true,  authorGuildIsNull: false).SetName("{m}(Author is guild member)"));

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_AuthorIsNotTrackable_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_AuthorIsNotTrackable_DoesNothing(
            MessageReceivedNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockUserService.Invocations.ShouldBeEmpty();
        }

        [TestCaseSource(nameof(HandleNotificationAsync_MessageReceivedNotification_AuthorIsTrackable_TestCaseData))]
        public async Task HandleNotificationAsync_MessageReceivedNotification_AuthorIsTrackable_TracksAuthor(
            MessageReceivedNotification notification)
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockUserService.ShouldHaveReceived(x => x
                .TrackUserAsync((IGuildUser)notification.Message.Author, testContext.CancellationToken));
        }

        #endregion HandleNotificationAsync(MessageReceivedNotification) Tests

        #region HandleNotificationAsync(UserJoinedNotification) Tests

        [Test]
        public async Task HandleNotificationAsync_UserJoinedNotification_Tests()
        {
            var testContext = new TestContext();

            var uut = testContext.BuildUut();

            var notification = new UserJoinedNotification(
                new Mock<ISocketGuildUser>().Object);

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockUserService.ShouldHaveReceived(x => x
                .TrackUserAsync(notification.GuildUser, testContext.CancellationToken));
        }

        #endregion HandleNotificationAsync(UserJoinedNotification) Tests
    }
}
