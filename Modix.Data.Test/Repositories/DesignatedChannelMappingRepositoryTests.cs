using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;

using NSubstitute;
using NUnit.Framework;

using Shouldly;

namespace Modix.Data.Test.Repositories
{
    public class DesignatedChannelMappingRepositoryTests
    {
        #region Test Context

        private static (ModixContext, DesignatedChannelMappingRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<UserEntity>().AddRange(Users.Entities.Clone());
                x.Set<GuildUserEntity>().AddRange(GuildUsers.Entities.Clone());
                x.Set<GuildChannelEntity>().AddRange(GuildChannels.Entities.Clone());
                x.Set<DesignatedChannelMappingEntity>().AddRange(DesignatedChannelMappings.Entities.Clone());
                x.Set<ConfigurationActionEntity>().AddRange(ConfigurationActions.Entities.Where(y => !(y.DesignatedChannelMappingId is null)).Clone());
            });

            var uut = new DesignatedChannelMappingRepository(modixContext);

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
        public async Task BeginCreateTransactionAsync_DeleteTransactionIsInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var deleteTransaction = await uut.BeginDeleteTransactionAsync();

            var result = uut.BeginCreateTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            deleteTransaction.Dispose();
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

            using (var transaction = await uut.BeginDeleteTransactionAsync()) { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginDeleteTransactionAsync() Tests

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_DoesNotUpdateDesignatedChannelMappingsAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null!));

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedChannelMappings.Entities.Select(x => x.Id));

            modixContext.Set<DesignatedChannelMappingEntity>().EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DesignatedChannelMappingCreationTestCases))]
        public async Task CreateAsync_DataIsNotNull_InsertsDesignatedChannelMapping(DesignatedChannelMappingCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var id = await uut.CreateAsync(data);

            modixContext.Set<DesignatedChannelMappingEntity>().ShouldContain(x => x.Id == id);
            var designatedChannelMapping = modixContext.Set<DesignatedChannelMappingEntity>().First(x => x.Id == id);

            designatedChannelMapping.GuildId.ShouldBe(data.GuildId);
            designatedChannelMapping.Type.ShouldBe(data.Type);
            designatedChannelMapping.ChannelId.ShouldBe(data.ChannelId);
            designatedChannelMapping.DeleteActionId.ShouldBeNull();

            modixContext.Set<DesignatedChannelMappingEntity>().Where(x => x.Id != designatedChannelMapping.Id).Select(x => x.Id).ShouldBe(DesignatedChannelMappings.Entities.Select(x => x.Id));
            modixContext.Set<DesignatedChannelMappingEntity>().Where(x => x.Id != designatedChannelMapping.Id).EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>().ShouldContain(x => x.Id == designatedChannelMapping.CreateActionId);
            var createAction = modixContext.Set<ConfigurationActionEntity>().First(x => x.Id == designatedChannelMapping.CreateActionId);

            createAction.GuildId.ShouldBe(data.GuildId);
            createAction.Type.ShouldBe(ConfigurationActionType.DesignatedChannelMappingCreated);
            createAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            createAction.CreatedById.ShouldBe(data.CreatedById);
            createAction.ClaimMappingId.ShouldBeNull();
            createAction.DesignatedChannelMappingId.ShouldBe(designatedChannelMapping.Id);
            createAction.DesignatedRoleMappingId.ShouldBeNull();

            modixContext.Set<ConfigurationActionEntity>().Where(x => x.Id != createAction.Id).Select(x => x.Id).ShouldBe(ConfigurationActions.Entities.Where(x => !(x.DesignatedChannelMappingId is null)).Select(x => x.Id));
            modixContext.Set<ConfigurationActionEntity>().Where(x => x.Id != createAction.Id).EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(2)
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region AnyAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaTestCases))]
        public async Task AnyAsync_DesignatedChannelMappingsExist_ReturnsTrue(DesignatedChannelMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria, default);

            result.ShouldBeTrue();
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task AnyAsync_DesignatedChannelMappingsDoNotExist_ReturnsFalse(DesignatedChannelMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria, default);

            result.ShouldBeFalse();
        }

        #endregion AnyAsync() Tests

        #region SearchChannelIdsAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndResultIdsTestCases))]
        public async Task SearchChannelIdsAsync_DesignatedChannelMappingsExist_ReturnsMatchingIds(DesignatedChannelMappingSearchCriteria criteria, long[] resultIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchChannelIdsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBe(resultIds.Select(x => DesignatedChannelMappings.Entities.First(y => y.Id == x).ChannelId));
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task SearchChannelIdsAsync_DesignatedChannelMappingsDoNotExist_ReturnsEmpty(DesignatedChannelMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchChannelIdsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        #endregion SearchChannelIdsAsync() Tests

        #region SearchBriefsAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndResultIdsTestCases))]
        public async Task SearchBriefsAsync_DesignatedChannelMappingsExist_ReturnsMatchingBriefs(DesignatedChannelMappingSearchCriteria criteria, long[] resultIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.Select(x => x.Id).ShouldBe(resultIds);
            result.EachShould(x => x.ShouldMatchTestData());
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task SearchBriefsAsync_DesignatedChannelMappingsDoNotExist_ReturnsEmpty(DesignatedChannelMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        #endregion SearchBriefsAsync() Tests

        #region DeleteAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndValidUserIdAndResultIdsTestCases))]
        public async Task DeleteAsync_DesignatedChannelMappingsAreNotDeleted_UpdatesDesignatedChannelMappingsAndReturnsCount(DesignatedChannelMappingSearchCriteria criteria, ulong deletedById, long[] designatedChannelMappingIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.DeleteAsync(criteria, deletedById);

            result.ShouldBe(designatedChannelMappingIds
                .Where(x => DesignatedChannelMappings.Entities
                    .Any(y => (y.Id == x) && (y.DeleteActionId == null)))
                .Count());

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedChannelMappings.Entities.Select(x => x.Id));

            modixContext.Set<DesignatedChannelMappingEntity>()
                .Where(x => designatedChannelMappingIds.Contains(x.Id) && (x.DeleteActionId == null))
                .EachShould(entity =>
                {
                    var originalEntity = DesignatedChannelMappings.Entities.First(x => x.Id == entity.Id);

                    entity.GuildId.ShouldBe(originalEntity.GuildId);
                    entity.ChannelId.ShouldBe(originalEntity.ChannelId);
                    entity.Type.ShouldBe(originalEntity.Type);
                    entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
                    entity.DeleteActionId.ShouldNotBeNull();

                    modixContext.Set<ConfigurationActionEntity>().ShouldContain(x => x.Id == entity.DeleteActionId);
                    var deleteAction = modixContext.Set<ConfigurationActionEntity>().First(x => x.Id == entity.DeleteActionId);

                    deleteAction.GuildId.ShouldBe(entity.GuildId);
                    deleteAction.Type.ShouldBe(ConfigurationActionType.DesignatedChannelMappingDeleted);
                    deleteAction.Created.ShouldBeInRange(
                        DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1),
                        DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1));
                    deleteAction.CreatedById.ShouldBe(deletedById);
                    deleteAction.ClaimMappingId.ShouldBeNull();
                    deleteAction.DesignatedChannelMappingId.ShouldBe(entity.Id);
                    deleteAction.DesignatedRoleMappingId.ShouldBeNull();
                });

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsEnumerable()
                .Where(x => !designatedChannelMappingIds.Contains(x.Id) || DesignatedChannelMappings.Entities
                    .Any(y => (y.Id == x.Id) && (x.DeleteActionId == null)))
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsEnumerable()
                .Where(x => DesignatedChannelMappings.Entities
                    .Any(y => (y.DeleteActionId == x.Id) && designatedChannelMappingIds.Contains(y.Id)))
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaAndValidUserIdTestCases))]
        public async Task DeleteAsync_DesignatedChannelMappingsDoNotExist_DoesNotUpdateDesignatedChannelMappingsAndReturns0(DesignatedChannelMappingSearchCriteria criteria, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.DeleteAsync(criteria, deletedById);

            result.ShouldBe(0);

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedChannelMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedChannelMappingEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => x.DesignatedChannelMappingId != null)
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        #endregion DeleteAsync() Tests

        #region TryDeleteAsync() Tests

        [TestCaseSource(nameof(ActiveDesignatedChannelMappingAndValidUserIdTestCases))]
        public async Task TryDeleteAsync_DesignatedChannelMappingIsNotDeleted_UpdatesDesignatedChannelMappingAndReturnsTrue(long designatedChannelMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(designatedChannelMappingId, deletedById);

            result.ShouldBeTrue();

            modixContext.Set<DesignatedChannelMappingEntity>()
                .ShouldContain(x => x.Id == designatedChannelMappingId);
            var designatedChannelMapping = modixContext.Set<DesignatedChannelMappingEntity>()
                .First(x => x.Id == designatedChannelMappingId);

            var originalDesignatedChannelMapping = DesignatedChannelMappings.Entities
                .First(x => x.Id == designatedChannelMappingId);

            designatedChannelMapping.GuildId.ShouldBe(originalDesignatedChannelMapping.GuildId);
            designatedChannelMapping.ChannelId.ShouldBe(originalDesignatedChannelMapping.ChannelId);
            designatedChannelMapping.Type.ShouldBe(originalDesignatedChannelMapping.Type);
            designatedChannelMapping.CreateActionId.ShouldBe(originalDesignatedChannelMapping.CreateActionId);
            designatedChannelMapping.DeleteActionId.ShouldNotBeNull();

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedChannelMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedChannelMappingEntity>()
                .Where(x => x.Id != designatedChannelMappingId)
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .ShouldContain(x => x.Id == designatedChannelMapping.DeleteActionId);
            var deleteAction = modixContext.Set<ConfigurationActionEntity>()
                .First(x => x.Id == designatedChannelMapping.DeleteActionId);

            deleteAction.GuildId.ShouldBe(designatedChannelMapping.GuildId);
            deleteAction.Type.ShouldBe(ConfigurationActionType.DesignatedChannelMappingDeleted);
            deleteAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            deleteAction.CreatedById.ShouldBe(deletedById);
            deleteAction.ClaimMappingId.ShouldBeNull();
            deleteAction.DesignatedChannelMappingId.ShouldBe(designatedChannelMapping.Id);
            deleteAction.DesignatedRoleMappingId.ShouldBeNull();

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.DesignatedChannelMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DeletedDesignatedChannelMappingAndValidUserIdTestCases))]
        [TestCaseSource(nameof(InvalidDesignatedChannelMappingAndValidUserIdTestCases))]
        public async Task TryDeleteAsync_DesignatedChannelMappingIsDeleted_DoesNotUpdateDesignatedChannelMappingsAndReturnsFalse(long designatedChannelMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(designatedChannelMappingId, deletedById);

            result.ShouldBeFalse();

            modixContext.Set<DesignatedChannelMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedChannelMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedChannelMappingEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.DesignatedChannelMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryDeleteAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> DesignatedChannelMappingCreationTestCases
            = DesignatedChannelMappings.Creations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.GuildId}, {x.ChannelId}, {x.Type})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaTestCases
            = DesignatedChannelMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaAndResultIdsTestCases
            = DesignatedChannelMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria, x.resultIds)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaAndValidUserIdAndResultIdsTestCases
            = DesignatedChannelMappings.Searches
                .Where(x => x.resultIds.Any())
                .SelectMany(x => Users.Entities
                    .Where(y => DesignatedChannelMappings.Entities
                        .Where(z => x.resultIds.Contains(z.Id))
                        .Select(z => z.GuildId)
                        .Distinct()
                        .All(z => GuildUsers.Entities
                            .Any(w => (w.UserId == y.Id) && (w.GuildId == z))))
                    .Select(y => new TestCaseData(x.criteria, y.Id, x.resultIds)
                        .SetName($"{{m}}(\"{x.name}\", {y.Id})")));

        public static readonly IEnumerable<TestCaseData> InvalidSearchCriteriaTestCases
            = DesignatedChannelMappings.Searches
                .Where(x => !x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> InvalidSearchCriteriaAndValidUserIdTestCases
            = DesignatedChannelMappings.Searches
                .Where(x => !x.resultIds.Any())
                .SelectMany(x => Users.Entities
                    .Select(y => new TestCaseData(x.criteria, y.Id)
                        .SetName($"{{m}}(\"{x.name}\", {y.Id})")));

        public static readonly IEnumerable<TestCaseData> ActiveDesignatedChannelMappingAndValidUserIdTestCases
            = DesignatedChannelMappings.Entities
                .Where(x => x.DeleteActionId is null)
                .SelectMany(x => GuildUsers.Entities
                    .Where(y => y.GuildId == x.GuildId)
                    .Select(y => new TestCaseData(x.Id, y.UserId)));

        public static readonly IEnumerable<TestCaseData> DeletedDesignatedChannelMappingAndValidUserIdTestCases
            = DesignatedChannelMappings.Entities
                .Where(x => !(x.DeleteActionId is null))
                .SelectMany(x => GuildUsers.Entities
                    .Where(y => y.GuildId == x.GuildId)
                    .Select(y => new TestCaseData(x.Id, y.UserId)));

        public static readonly IEnumerable<TestCaseData> InvalidDesignatedChannelMappingAndValidUserIdTestCases
            = Enumerable.Empty<long>()
                .Append(DesignatedChannelMappings.Entities
                    .Select(x => x.Id)
                    .Max() + 1)
                .SelectMany(x => Users.Entities
                    .Select(y => new TestCaseData(x, y.Id)));

        #endregion Test Data
    }
}
