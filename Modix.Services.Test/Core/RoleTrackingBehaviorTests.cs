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

using Modix.Services.Core;

using Modix.Common.Test;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class RoleTrackingBehaviorTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodTestContext
        {
            public TestContext(
                IReadOnlyCollection<ISocketGuild>? guilds = null)
            {
                MockDiscordSocketClient = new Mock<IDiscordSocketClient>();
                MockDiscordSocketClient
                    .Setup(x => x.Guilds)
                    .Returns(guilds ?? Array.Empty<ISocketGuild>());

                MockRoleService = new Mock<IRoleService>();
            }

            public RoleTrackingBehavior BuildUut()
                => new RoleTrackingBehavior(
                    MockDiscordSocketClient.Object,
                    MockRoleService.Object);

            public readonly Mock<IDiscordSocketClient> MockDiscordSocketClient;
            public readonly Mock<IRoleService> MockRoleService;
        }

        #endregion Test Context

        #region HandleNotificationAsync(JoinedGuildNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(
            IReadOnlyCollection<ulong> roleIds)
        {
            var roles = roleIds
                .Select(roleId =>
                {
                    var mockRole = new Mock<ISocketRole>();
                    mockRole
                        .Setup(x => x.Id)
                        .Returns(roleId);

                    return mockRole.Object;
                })
                .ToArray();

            var mockGuild = new Mock<ISocketGuild>();
            mockGuild
                .Setup(x => x.Roles)
                .Returns(roles);

            var notification = new JoinedGuildNotification(
                mockGuild.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_JoinedGuildNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { default(ulong) }   ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { ulong.MinValue }   ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { ulong.MaxValue }   ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { 1UL            }   ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { 2UL, 3UL       }   ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: new[] { 4UL, 5UL, 6UL  }   ).SetName("{m}(Unique Values 3)"),
                BuildTestCaseData_HandleNotificationAsync_JoinedGuildNotification(  roleIds: Array.Empty<ulong>()       ).SetName("{m}(Roles is empty)"));

        [TestCaseSource(nameof(HandleNotificationAsync_JoinedGuildNotification_TestCaseData))]
        public async Task HandleNotificationAsync_JoinedGuildNotification_Always_TracksRoles(
            JoinedGuildNotification notification)
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            foreach(var role in notification.Guild.Roles)
                testContext.MockRoleService.ShouldHaveReceived(x => x
                    .TrackRoleAsync(role, testContext.CancellationToken));
            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(It.IsAny<IRole>(), It.IsAny<CancellationToken>()), Times.Exactly(notification.Guild.Roles.Count));
        }

        #endregion HandleNotificationAsync(JoinedGuildNotification) Tests

        #region HandleNotificationAsync(ReadyNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_ReadyNotification(
            IReadOnlyCollection<IReadOnlyCollection<ulong>> roleIdsByGuild)
        {
            var guilds = roleIdsByGuild
                .Select(roleIds =>
                {
                    var roles = roleIds
                        .Select(roleId =>
                        {
                            var mockRole = new Mock<ISocketRole>();
                            mockRole
                                .Setup(x => x.Id)
                                .Returns(roleId);

                            return mockRole.Object;
                        })
                        .ToArray();

                    var mockGuild = new Mock<ISocketGuild>();
                    mockGuild
                        .Setup(x => x.Roles)
                        .Returns(roles);

                    return mockGuild.Object;
                })
                .ToArray();

            var notification = ReadyNotification.Default;

            return new TestCaseData(guilds, notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_ReadyNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { default(ulong) }                                               }    ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { ulong.MinValue }                                               }    ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { ulong.MaxValue }                                               }    ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { 1UL            }                                               }    ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { 2UL            }, new[] { 3UL, 4UL }                           }    ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { new[] { 5UL            }, new[] { 6UL, 7UL }, new[] { 8UL, 9UL, 10UL } }    ).SetName("{m}(Unique Values 3)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: Array.Empty<IReadOnlyCollection<ulong>>()                                           ).SetName("{m}(Guilds is empty)"),
                BuildTestCaseData_HandleNotificationAsync_ReadyNotification(    roleIdsByGuild: new[] { Array.Empty<ulong>()                                                   }    ).SetName("{m}(Roles is empty)"));

        [TestCaseSource(nameof(HandleNotificationAsync_ReadyNotification_TestCaseData))]
        public async Task HandleNotificationAsync_ReadyNotification_Always_TracksGuildsRoles(
            IReadOnlyCollection<ISocketGuild> guilds,
            ReadyNotification notification)
        {
            using var testContext = new TestContext(
                guilds);

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            var roles = guilds.SelectMany(x => x.Roles)
                .ToArray();

            foreach (var role in roles)
                testContext.MockRoleService.ShouldHaveReceived(x => x
                    .TrackRoleAsync(role, testContext.CancellationToken));
            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(It.IsAny<IRole>(), It.IsAny<CancellationToken>()), Times.Exactly(roles.Length));
        }

        #endregion HandleNotificationAsync(ReadyNotification) Tests

        #region HandleNotificationAsync(RoleCreatedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(
            ulong roleId)
        {
            var mockRole = new Mock<ISocketRole>();
            mockRole
                .Setup(x => x.Id)
                .Returns(roleId);

            var notification = new RoleCreatedNotification(
                mockRole.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_RoleCreatedNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: default         ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: ulong.MinValue  ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: ulong.MaxValue  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: 1UL             ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: 2UL             ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_RoleCreatedNotification(  roleId: 3UL             ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_RoleCreatedNotification_TestCaseData))]
        public async Task HandleNotificationAsync_RoleCreatedNotification_Always_TracksRole(
            RoleCreatedNotification notification)
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(notification.Role, testContext.CancellationToken));
            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(It.IsAny<IRole>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion HandleNotificationAsync(RoleCreatedNotification) Tests

        #region HandleNotificationAsync(RoleUpdatedNotification) Tests

        public static TestCaseData BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(
            ulong roleId)
        {
            var mockOldRole = new Mock<ISocketRole>();
            mockOldRole
                .Setup(x => x.Id)
                .Returns(roleId);

            var mockNewRole = new Mock<ISocketRole>();
            mockNewRole
                .Setup(x => x.Id)
                .Returns(roleId);

            var notification = new RoleUpdatedNotification(
                mockOldRole.Object,
                mockNewRole.Object);

            return new TestCaseData(notification);
        }

        public static readonly ImmutableArray<TestCaseData> HandleNotificationAsync_RoleUpdatedNotification_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: default         ).SetName("{m}(Default Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: ulong.MinValue  ).SetName("{m}(Min Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: ulong.MaxValue  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: 1UL             ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: 2UL             ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_HandleNotificationAsync_RoleUpdatedNotification(  roleId: 3UL             ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(HandleNotificationAsync_RoleUpdatedNotification_TestCaseData))]
        public async Task HandleNotificationAsync_RoleUpdatedNotification_Always_TracksRole(
            RoleUpdatedNotification notification)
        {
            using var testContext = new TestContext();

            var uut = testContext.BuildUut();

            await uut.HandleNotificationAsync(
                notification,
                testContext.CancellationToken);

            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(notification.NewRole, testContext.CancellationToken));
            testContext.MockRoleService.ShouldHaveReceived(x => x
                .TrackRoleAsync(It.IsAny<IRole>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion HandleNotificationAsync(RoleUpdatedNotification) Tests
    }
}
