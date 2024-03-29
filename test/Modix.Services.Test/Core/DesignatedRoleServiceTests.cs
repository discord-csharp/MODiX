using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    public class DesignatedRoleServiceTests
    {
        #region Test Context

        public class TestContext
            : AsyncMethodTestContext
        {
            public TestContext()
            {
                MockAuthorizationService = new Mock<IAuthorizationService>();

                MockDesignatedRoleMappingRepository = new Mock<IDesignatedRoleMappingRepository>();
            }

            public DesignatedRoleService BuildUut()
                => new DesignatedRoleService(
                    MockAuthorizationService.Object,
                    MockDesignatedRoleMappingRepository.Object);

            public readonly Mock<IAuthorizationService> MockAuthorizationService;
            public readonly Mock<IDesignatedRoleMappingRepository> MockDesignatedRoleMappingRepository;
        }

        #endregion Test Context

        #region RoleHasDesignationAsync() Tests

        public static readonly ImmutableArray<TestCaseData> RoleHasDesignationAsync_TestCaseData
            = ImmutableArray.Create(
                /*                  guildId,        roleId,         designation,                                    expectedResult  */
                new TestCaseData(   default(ulong), default(ulong), default(DesignatedRoleType),                    default(bool)   ).SetName("{m}(Default Values)"),
                new TestCaseData(   ulong.MinValue, ulong.MinValue, DesignatedRoleType.Rank,                        false           ).SetName("{m}(Min Values)"),
                new TestCaseData(   ulong.MaxValue, ulong.MaxValue, DesignatedRoleType.Pingable,                    true            ).SetName("{m}(Max Values)"),
                new TestCaseData(   1UL,            2UL,            DesignatedRoleType.Rank,                        false           ).SetName("{m}(Unique Values 1)"),
                new TestCaseData(   3UL,            4UL,            DesignatedRoleType.ModerationMute,              true            ).SetName("{m}(Unique Values 2)"));

        [TestCaseSource(nameof(RoleHasDesignationAsync_TestCaseData))]
        public async Task RoleHasDesignationAsync_Always_ReturnsDesignatedRoleMappingRepositoryAnyAsync(
            ulong guildId,
            ulong roleId,
            DesignatedRoleType designation,
            bool expectedResult)
        {
            using var testContext = new TestContext();

            testContext.MockDesignatedRoleMappingRepository
                .Setup(x => x.AnyAsync(It.IsAny<DesignatedRoleMappingSearchCriteria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var uut = testContext.BuildUut();

            var result = await uut.RoleHasDesignationAsync(
                guildId,
                roleId,
                designation,
                testContext.CancellationToken);

            testContext.MockAuthorizationService.Invocations.ShouldBeEmpty();

            testContext.MockDesignatedRoleMappingRepository.ShouldHaveReceived(x => x
                .AnyAsync(It.IsNotNull<DesignatedRoleMappingSearchCriteria>(), It.IsAny<CancellationToken>()));

            var criteria = testContext.MockDesignatedRoleMappingRepository.Invocations
                .Where(x => x.Method.Name == nameof(IDesignatedRoleMappingRepository.AnyAsync))
                .Select(x => (DesignatedRoleMappingSearchCriteria)x.Arguments[0])
                .First();

            criteria.GuildId.ShouldBe(guildId);
            criteria.RoleId.ShouldBe(roleId);
            criteria.Type.ShouldBe(designation);
            criteria.IsDeleted.ShouldBe(false);

            criteria.Id.ShouldBeNull();
            criteria.RoleIds.ShouldBeNull();
            criteria.CreatedById.ShouldBeNull();

            result.ShouldBe(expectedResult);
        }

        #endregion RoleHasDesignationAsync() Tests

        #region RolesHaveDesignationAsync() Tests

        public static readonly ImmutableArray<TestCaseData> RolesHaveDesignationAsync_TestCaseData
            = ImmutableArray.Create(
                /*                  guildId,        roleIds,                    designation,                                    expectedResult  */
                new TestCaseData(   default(ulong), new[] { default(ulong) },   default(DesignatedRoleType),                    default(bool)   ).SetName("{m}(Default Values)"),
                new TestCaseData(   ulong.MinValue, new[] { ulong.MinValue },   DesignatedRoleType.Rank,                        false           ).SetName("{m}(Min Values)"),
                new TestCaseData(   ulong.MaxValue, new[] { ulong.MaxValue },   DesignatedRoleType.Pingable,                    true            ).SetName("{m}(Max Values)"),
                new TestCaseData(   1UL,            Array.Empty<ulong>(),       DesignatedRoleType.Rank,                        false           ).SetName("{m}(Unique Values 1)"),
                new TestCaseData(   2UL,            new[] { 3UL },              DesignatedRoleType.ModerationMute,              true            ).SetName("{m}(Unique Values 2)"));

        [TestCaseSource(nameof(RolesHaveDesignationAsync_TestCaseData))]
        public async Task RolesHaveDesignationAsync_Always_ReturnsDesignatedRoleMappingRepositoryAnyAsync(
            ulong guildId,
            IReadOnlyCollection<ulong> roleIds,
            DesignatedRoleType designation,
            bool expectedResult)
        {
            using var testContext = new TestContext();

            testContext.MockDesignatedRoleMappingRepository
                .Setup(x => x.AnyAsync(It.IsAny<DesignatedRoleMappingSearchCriteria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var uut = testContext.BuildUut();

            var result = await uut.RolesHaveDesignationAsync(
                guildId,
                roleIds,
                designation,
                testContext.CancellationToken);

            testContext.MockAuthorizationService.Invocations.ShouldBeEmpty();

            testContext.MockDesignatedRoleMappingRepository.ShouldHaveReceived(x => x
                .AnyAsync(It.IsNotNull<DesignatedRoleMappingSearchCriteria>(), It.IsAny<CancellationToken>()));

            var criteria = testContext.MockDesignatedRoleMappingRepository.Invocations
                .Where(x => x.Method.Name == nameof(IDesignatedRoleMappingRepository.AnyAsync))
                .Select(x => (DesignatedRoleMappingSearchCriteria)x.Arguments[0])
                .First();

            criteria.GuildId.ShouldBe(guildId);
            criteria.RoleIds.ShouldBeSetEqualTo(roleIds);
            criteria.Type.ShouldBe(designation);
            criteria.IsDeleted.ShouldBe(false);

            criteria.Id.ShouldBeNull();
            criteria.RoleId.ShouldBeNull();
            criteria.CreatedById.ShouldBeNull();

            result.ShouldBe(expectedResult);
        }

        #endregion RolesHaveDesignationAsync() Tests
    }
}
