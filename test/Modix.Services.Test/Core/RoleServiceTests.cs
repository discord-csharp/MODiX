#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Moq;
using NUnit.Framework;
using Shouldly;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;

using Modix.Common.Test;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class RoleServiceTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodWithLoggerTestContext
        {
            public TestContext(
                bool roleExists)
            {
                GuildRoleMutationData = new GuildRoleMutationData();

                MockCreateTransaction = new Mock<IRepositoryTransaction>();

                MockGuildRoleRepository = new Mock<IGuildRoleRepository>();
                MockGuildRoleRepository
                    .Setup(x => x.BeginCreateTransactionAsync())
                    .ReturnsAsync(() => MockCreateTransaction.Object);
                MockGuildRoleRepository
                    .Setup(x => x.TryUpdateAsync(It.IsAny<ulong>(), It.IsAny<Action<GuildRoleMutationData>>()))
                    .Returns<ulong, Action<GuildRoleMutationData>>((roleId, updateAction) =>
                    {
                        if (roleExists)
                            updateAction.Invoke(GuildRoleMutationData);

                        return Task.FromResult(roleExists);
                    });
            }

            public RoleService BuildUut()
                => new RoleService(
                    MockGuildRoleRepository.Object,
                    LoggerFactory.CreateLogger<RoleService>());

            public readonly GuildRoleMutationData GuildRoleMutationData;
            public readonly Mock<IRepositoryTransaction> MockCreateTransaction;
            public readonly Mock<IGuildRoleRepository> MockGuildRoleRepository;
        }

        #endregion Test Context

        #region TrackRoleAsync() Tests

        public static TestCaseData BuildTestCaseData_TrackRoleAsync(
            ulong guildId,
            ulong roleId,
            string roleName,
            int rolePosition)
        {
            var mockGuild = new Mock<IGuild>();
            mockGuild
                .Setup(x => x.Id)
                .Returns(guildId);

            var mockRole = new Mock<IRole>();
            mockRole
                .Setup(x => x.Id)
                .Returns(roleId);
            mockRole
                .Setup(x => x.Name)
                .Returns(roleName);
            mockRole
                .Setup(x => x.Position)
                .Returns(rolePosition);
            mockRole
                .Setup(x => x.Guild)
                .Returns(mockGuild.Object);

            return new TestCaseData(mockRole.Object);
        }

        public static readonly ImmutableArray<TestCaseData> TrackRoleAsync_TestCaseData
            = ImmutableArray.Create(
                BuildTestCaseData_TrackRoleAsync(   guildId: default,           roleId: default,        roleName: string.Empty, rolePosition: default       ).SetName("{m}(Default Values)"),
                BuildTestCaseData_TrackRoleAsync(   guildId: ulong.MinValue,    roleId: ulong.MinValue, roleName: string.Empty, rolePosition: int.MinValue  ).SetName("{m}(Min Values)"),
                BuildTestCaseData_TrackRoleAsync(   guildId: ulong.MaxValue,    roleId: ulong.MaxValue, roleName: string.Empty, rolePosition: int.MaxValue  ).SetName("{m}(Max Values)"),
                BuildTestCaseData_TrackRoleAsync(   guildId: 1UL,               roleId: 2UL,            roleName: "3",          rolePosition: 4             ).SetName("{m}(Unique Values 1)"),
                BuildTestCaseData_TrackRoleAsync(   guildId: 5UL,               roleId: 6UL,            roleName: "7",          rolePosition: 8             ).SetName("{m}(Unique Values 2)"),
                BuildTestCaseData_TrackRoleAsync(   guildId: 9UL,               roleId: 10UL,           roleName: "11",         rolePosition: 12            ).SetName("{m}(Unique Values 3)"));

        [TestCaseSource(nameof(TrackRoleAsync_TestCaseData))]
        public async Task TrackRoleAsync_RoleUpdateSucceeds_DoesNotCreateRole(
            IRole role)
        {
            using var testContext = new TestContext(
                roleExists: true);

            var uut = testContext.BuildUut();

            await uut.TrackRoleAsync(
                role,
                testContext.CancellationToken);

            testContext.MockGuildRoleRepository.ShouldHaveReceived(x => x
                .BeginCreateTransactionAsync());

            testContext.MockGuildRoleRepository.ShouldHaveReceived(x => x
                .TryUpdateAsync(role.Id, It.IsNotNull<Action<GuildRoleMutationData>>()));
            testContext.GuildRoleMutationData.Name      .ShouldBe(role.Name);
            testContext.GuildRoleMutationData.Position  .ShouldBe(role.Position);

            testContext.MockGuildRoleRepository.ShouldNotHaveReceived(x => x
                .CreateAsync(It.IsAny<GuildRoleCreationData>()));

            testContext.MockCreateTransaction.ShouldHaveReceived(x => x
                .Dispose());
        }

        [TestCaseSource(nameof(TrackRoleAsync_TestCaseData))]
        public async Task TrackRoleAsync_RoleUpdateFails_CreatesRole(
            IRole role)
        {
            using var testContext = new TestContext(
                roleExists: false);

            var uut = testContext.BuildUut();

            await uut.TrackRoleAsync(
                role,
                testContext.CancellationToken);

            testContext.MockGuildRoleRepository.ShouldHaveReceived(x => x
                .BeginCreateTransactionAsync());

            testContext.MockGuildRoleRepository.ShouldHaveReceived(x => x
                .TryUpdateAsync(role.Id, It.IsNotNull<Action<GuildRoleMutationData>>()));

            testContext.MockGuildRoleRepository.ShouldHaveReceived(x => x
                .CreateAsync(It.IsNotNull<GuildRoleCreationData>()));
            var creationData = testContext.MockGuildRoleRepository.Invocations
                .Where(x => x.Method.Name == nameof(IGuildRoleRepository.CreateAsync))
                .Select(x => (GuildRoleCreationData)x.Arguments[0])
                .First();
            creationData.RoleId     .ShouldBe(role.Id);
            creationData.GuildId    .ShouldBe(role.Guild.Id);
            creationData.Name       .ShouldBe(role.Name);
            creationData.Position   .ShouldBe(role.Position);

            testContext.MockCreateTransaction.ShouldHaveReceived(x => x
                .Dispose());
        }

        #endregion TrackRoleAsync() Tests
    }
}
