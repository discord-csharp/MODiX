using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;

using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class ClaimMappingRepositoryTests
    {
        #region Test Context

        private static (ModixContext, ClaimMappingRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<UserEntity>().AddRange(Users.Entities.Clone());
                x.Set<GuildUserEntity>().AddRange(GuildUsers.Entities.Clone());
                x.Set<ClaimMappingEntity>().AddRange(ClaimMappings.Entities.Clone());
                x.Set<ConfigurationActionEntity>().AddRange(ConfigurationActions.Entities.Where(y => !(y.ClaimMappingId is null)).Clone());
            });

            var uut = new ClaimMappingRepository(modixContext);

            return (modixContext, uut);
        }

        #endregion Test Context

        #region Constructor() Tests

        [Test]
        public void Constructor_Always_InvokesBaseConstructor()
        {
            (var modixContext, var uut) = BuildTestContext();

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }

        #endregion Constructor() Tests

        #region BeginCreateTransactionAsync() Tests

        [Test]
        [NonParallelizable]
        public async Task BeginCreateTransactionAsync_CreateTransactionIsInProgress_WaitsForCompletion()
        {
            (var modixContext, var uut) = BuildTestContext();

            var existingTransaction = await uut.BeginCreateTransactionAsync();

            var result = uut.BeginCreateTransactionAsync();

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginCreateTransactionAsync_CreateTransactionIsNotInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = uut.BeginCreateTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginCreateTransactionAsync_Always_TransactionIsForContextDatabase()
        {
            (var modixContext, var uut) = BuildTestContext();

            var database = Substitute.ForPartsOf<DatabaseFacade>(modixContext);
            modixContext.Database.Returns(database);

            using (var transaction = await uut.BeginCreateTransactionAsync()) { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginCreateTransactionAsync() Tests

        #region BeginDeleteTransactionAsync() Tests

        [Test]
        [NonParallelizable]
        public async Task BeginDeleteTransactionAsync_DeleteTransactionIsInProgress_WaitsForCompletion()
        {
            (var modixContext, var uut) = BuildTestContext();

            var existingTransaction = await uut.BeginDeleteTransactionAsync();

            var result = uut.BeginDeleteTransactionAsync();

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginDeleteTransactionAsync_DeleteTransactionIsNotInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = uut.BeginDeleteTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginDeleteTransactionAsync_CreateTransactionIsInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var createTransaction = await uut.BeginCreateTransactionAsync();

            var result = uut.BeginDeleteTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            createTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginDeleteTransactionAsync_Always_TransactionIsForContextDatabase()
        {
            (var modixContext, var uut) = BuildTestContext();

            var database = Substitute.ForPartsOf<DatabaseFacade>(modixContext);
            modixContext.Database.Returns(database);

            using (var transaction = await uut.BeginDeleteTransactionAsync())
            { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginDeleteTransactionAsync() Tests

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_ThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null!));

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ClaimMappingCreationTestCases))]
        public async Task CreateAsync_DataIsNotNull_InsertsClaimMapping(ClaimMappingCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var id = await uut.CreateAsync(data);

            modixContext.Set<ClaimMappingEntity>().ShouldContain(x => x.Id == id);
            var claimMapping = modixContext.Set<ClaimMappingEntity>().First(x => x.Id == id);

            claimMapping.GuildId.ShouldBe(data.GuildId);
            claimMapping.Type.ShouldBe(data.Type);
            claimMapping.RoleId.ShouldBe(data.RoleId);
            claimMapping.UserId.ShouldBe(data.UserId);
            claimMapping.Claim.ShouldBe(data.Claim);

            modixContext.Set<ConfigurationActionEntity>()
                .ShouldContain(x => x.Id == claimMapping.CreateActionId);
            var createAction = modixContext.Set<ConfigurationActionEntity>()
                .First(x => x.Id == claimMapping.CreateActionId);

            createAction.GuildId.ShouldBe(data.GuildId);
            createAction.Type.ShouldBe(ConfigurationActionType.ClaimMappingCreated);
            createAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            createAction.CreatedById.ShouldBe(data.CreatedById);
            createAction.ClaimMappingId.ShouldBe(id);
            createAction.DesignatedChannelMappingId.ShouldBeNull();
            createAction.DesignatedRoleMappingId.ShouldBeNull();

            await modixContext.ShouldHaveReceived(2)
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region ReadAsync() Tests

        [TestCaseSource(nameof(ValidClaimMappingIds))]
        public async Task ReadAsync_ClaimMappingExists_ReturnsMatchingClaimMappingSummary(long claimMappingId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadAsync(claimMappingId);

            result.ShouldMatchTestData();
        }

        [TestCaseSource(nameof(InvalidClaimMappingIds))]
        public async Task ReadAsync_ClaimMappingDoesNotExist_ReturnsNull(long claimMappingId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadAsync(claimMappingId);

            result.ShouldBeNull();
        }

        #endregion ReadAsync() Tests

        #region AnyAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaTestCases))]
        public async Task AnyAsync_ClaimMappingsExist_ReturnsTrue(ClaimMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria);

            result.ShouldBeTrue();
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task AnyAsync_ClaimMappingsDoNotExist_ReturnsFalse(ClaimMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria);

            result.ShouldBeFalse();
        }

        #endregion AnyAsync() Tests

        #region SearchIdsAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndResultIdsTestCases))]
        public async Task SearchIdsAsync_ClaimMappingsExist_ReturnsMatchingIds(ClaimMappingSearchCriteria criteria, long[] resultIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchIdsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBe(resultIds);
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task SearchIdsAsync_ClaimMappingsDoNotExist_ReturnsEmpty(ClaimMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchIdsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        #endregion SearchIdsAsync() Tests

        #region SearchBriefsAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndResultIdsTestCases))]
        public async Task SearchBriefsAsync_ClaimMappingsExist_ReturnsMatchingBriefs(ClaimMappingSearchCriteria criteria, long[] resultIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.Select(x => x.Id).ShouldBe(resultIds);
            result.EachShould(x => x.ShouldMatchTestData());
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task SearchBriefsAsync_ClaimMappingsDoNotExist_ReturnsEmpty(ClaimMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        #endregion SearchBriefsAsync() Tests

        #region TryDeleteAsync() Tests

        [TestCaseSource(nameof(ActiveClaimMappingWithValidUserIdTestCases))]
        public async Task TryDeleteAsync_ClaimMappingIsNotDeleted_UpdatesClaimMappingAndReturnsTrue(long claimMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(claimMappingId, deletedById);

            result.ShouldBeTrue();

            modixContext.Set<ClaimMappingEntity>()
                .ShouldContain(x => x.Id == claimMappingId);
            var claimMapping = modixContext.Set<ClaimMappingEntity>()
                .First(x => x.Id == claimMappingId);

            var originalClaimMapping = ClaimMappings.Entities
                .First(x => x.Id == claimMappingId);

            claimMapping.GuildId.ShouldBe(originalClaimMapping.GuildId);
            claimMapping.Type.ShouldBe(originalClaimMapping.Type);
            claimMapping.RoleId.ShouldBe(originalClaimMapping.RoleId);
            claimMapping.UserId.ShouldBe(originalClaimMapping.UserId);
            claimMapping.Claim.ShouldBe(originalClaimMapping.Claim);
            claimMapping.CreateActionId.ShouldBe(originalClaimMapping.CreateActionId);
            claimMapping.DeleteActionId.ShouldNotBeNull();

            modixContext.Set<ClaimMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ClaimMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<ClaimMappingEntity>()
                .Where(x => x.Id != claimMappingId)
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .ShouldContain(x => x.Id == claimMapping.DeleteActionId);
            var deleteAction = modixContext.Set<ConfigurationActionEntity>()
                .First(x => x.Id == claimMapping.DeleteActionId);

            deleteAction.GuildId.ShouldBe(claimMapping.GuildId);
            deleteAction.Type.ShouldBe(ConfigurationActionType.ClaimMappingDeleted);
            deleteAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            deleteAction.CreatedById.ShouldBe(deletedById);
            deleteAction.ClaimMappingId.ShouldBe(claimMapping.Id);
            deleteAction.DesignatedChannelMappingId.ShouldBeNull();
            deleteAction.DesignatedRoleMappingId.ShouldBeNull();

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.ClaimMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DeletedClaimMappingWithValidUserIdTestCases))]
        [TestCaseSource(nameof(InvalidClaimMappingWithValidUserIdTestCases))]
        public async Task TryDeleteAsync_ClaimMappingExists_DoesNotUpdateClaimMappingsAndReturnsFalse(long claimMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(claimMappingId, deletedById);

            result.ShouldBeFalse();

            modixContext.Set<ClaimMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ClaimMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<ClaimMappingEntity>()
                .Where(x => x.Id != claimMappingId)
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.ClaimMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryDeleteAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> ClaimMappingCreationTestCases
            = ClaimMappings.Creations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.GuildId}, {x.Type.ToString()}, {x.RoleId?.ToString() ?? "null"}, {x.UserId?.ToString() ?? "null"}, {x.Claim.ToString()})"));

        public static readonly IEnumerable<long> ValidClaimMappingIds
            = ClaimMappings.Entities
                .Select(x => x.Id);

        public static readonly IEnumerable<long> InvalidClaimMappingIds
            = Enumerable.Empty<long>()
                .Append(ClaimMappings.Entities.Max(x => x.Id) + 1);

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaTestCases
            = ClaimMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaAndResultIdsTestCases
            = ClaimMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria, x.resultIds)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> InvalidSearchCriteriaTestCases
            = ClaimMappings.Searches
                .Where(x => !x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ActiveClaimMappingWithValidUserIdTestCases
            = ClaimMappings.Entities
                .Where(x => x.DeleteActionId is null)
                .SelectMany(x => Users.Entities
                    .Where(y => GuildUsers.Entities.Any(z => (z.UserId == y.Id) && (z.GuildId == x.GuildId)))
                    .Select(y => new TestCaseData(x.Id, y.Id)));

        public static readonly IEnumerable<TestCaseData> DeletedClaimMappingWithValidUserIdTestCases
            = ClaimMappings.Entities
                .Where(x => !(x.DeleteActionId is null))
                .SelectMany(x => Users.Entities
                    .Where(y => GuildUsers.Entities.Any(z => z.GuildId == x.GuildId))
                    .Select(y => new TestCaseData(x.Id, y.Id)));

        public static readonly IEnumerable<TestCaseData> InvalidClaimMappingWithValidUserIdTestCases
            = InvalidClaimMappingIds
                .SelectMany(x => Users.Entities
                    .Select(y => new TestCaseData(x, y.Id)));

        #endregion Test Data
    }
}
