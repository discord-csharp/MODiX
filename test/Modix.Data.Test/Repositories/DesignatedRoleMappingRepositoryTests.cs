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
    public class DesignatedRoleMappingRepositoryTests
    {
        #region Test Context

        private static (ModixContext, DesignatedRoleMappingRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<UserEntity>().AddRange(Users.Entities.Clone());
                x.Set<GuildUserEntity>().AddRange(GuildUsers.Entities.Clone());
                x.Set<GuildRoleEntity>().AddRange(GuildRoles.Entities.Clone());
                x.Set<DesignatedRoleMappingEntity>().AddRange(DesignatedRoleMappings.Entities.Clone());
                x.Set<ConfigurationActionEntity>().AddRange(ConfigurationActions.Entities.Where(y => !(y.DesignatedRoleMappingId is null)).Clone());
            });

            var uut = new DesignatedRoleMappingRepository(modixContext);

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
        public async Task CreateAsync_DataIsNull_DoesNotUpdateDesignatedRoleMappingsAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null!));

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsQueryable().Select(x => x.Id).ShouldBe(DesignatedRoleMappings.Entities.Select(x => x.Id));
            modixContext.Set<DesignatedRoleMappingEntity>().EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DesignatedRoleMappingCreationTestCases))]
        public async Task CreateAsync_DataIsNotNull_InsertsDesignatedRoleMapping(DesignatedRoleMappingCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var id = await uut.CreateAsync(data);

            modixContext.Set<DesignatedRoleMappingEntity>().ShouldContain(x => x.Id == id);
            var designatedRoleMapping = modixContext.Set<DesignatedRoleMappingEntity>().First(x => x.Id == id);

            designatedRoleMapping.GuildId.ShouldBe(data.GuildId);
            designatedRoleMapping.Type.ShouldBe(data.Type);
            designatedRoleMapping.RoleId.ShouldBe(data.RoleId);
            designatedRoleMapping.DeleteActionId.ShouldBeNull();

            modixContext.Set<DesignatedRoleMappingEntity>().Where(x => x.Id != designatedRoleMapping.Id).Select(x => x.Id).ShouldBe(DesignatedRoleMappings.Entities.Select(x => x.Id));
            modixContext.Set<DesignatedRoleMappingEntity>().Where(x => x.Id != designatedRoleMapping.Id).EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>().ShouldContain(x => x.Id == designatedRoleMapping.CreateActionId);
            var createAction = modixContext.Set<ConfigurationActionEntity>().First(x => x.Id == designatedRoleMapping.CreateActionId);

            createAction.GuildId.ShouldBe(data.GuildId);
            createAction.Type.ShouldBe(ConfigurationActionType.DesignatedRoleMappingCreated);
            createAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            createAction.CreatedById.ShouldBe(data.CreatedById);
            createAction.DesignatedChannelMappingId.ShouldBeNull();
            createAction.DesignatedRoleMappingId.ShouldNotBeNull();
            createAction.DesignatedRoleMappingId.ShouldBe(designatedRoleMapping.Id);

            modixContext.Set<ConfigurationActionEntity>().Where(x => x.Id != createAction.Id).Select(x => x.Id).ShouldBe(ConfigurationActions.Entities.Where(x => !(x.DesignatedRoleMappingId is null)).Select(x => x.Id));
            modixContext.Set<ConfigurationActionEntity>().Where(x => x.Id != createAction.Id).EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(2)
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region AnyAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaTestCases))]
        public async Task AnyAsync_DesignatedRoleMappingsExist_ReturnsTrue(DesignatedRoleMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria, default);

            result.ShouldBeTrue();
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task AnyAsync_DesignatedRoleMappingsDoNotExist_ReturnsFalse(DesignatedRoleMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.AnyAsync(criteria, default);

            result.ShouldBeFalse();
        }

        #endregion AnyAsync() Tests

        #region SearchBriefsAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndResultIdsTestCases))]
        public async Task SearchBriefsAsync_DesignatedRoleMappingsExist_ReturnsMatchingBriefs(DesignatedRoleMappingSearchCriteria criteria, long[] resultIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.Select(x => x.Id).ShouldBe(resultIds);
            result.EachShould(x => x.ShouldMatchTestData());
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaTestCases))]
        public async Task SearchBriefsAsync_DesignatedRoleMappingsDoNotExist_ReturnsEmpty(DesignatedRoleMappingSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.SearchBriefsAsync(criteria);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        #endregion SearchBriefsAsync() Tests

        #region DeleteAsync() Tests

        [TestCaseSource(nameof(ValidSearchCriteriaAndValidUserIdAndResultIdsTestCases))]
        public async Task DeleteAsync_DesignatedRoleMappingsAreNotDeleted_UpdatesDesignatedRoleMappingsAndReturnsCount(DesignatedRoleMappingSearchCriteria criteria, ulong deletedById, long[] designatedRoleMappingIds)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.DeleteAsync(criteria, deletedById);

            result.ShouldBe(designatedRoleMappingIds
                .Where(x => DesignatedRoleMappings.Entities
                    .Any(y => (y.Id == x) && (y.DeleteActionId == null)))
                .Count());

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedRoleMappings.Entities.Select(x => x.Id));

            modixContext.Set<DesignatedRoleMappingEntity>()
                .Where(x => designatedRoleMappingIds.Contains(x.Id) && (x.DeleteActionId == null))
                .EachShould(entity =>
                {
                    var originalEntity = DesignatedRoleMappings.Entities.First(x => x.Id == entity.Id);

                    entity.GuildId.ShouldBe(originalEntity.GuildId);
                    entity.RoleId.ShouldBe(originalEntity.RoleId);
                    entity.Type.ShouldBe(originalEntity.Type);
                    entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
                    entity.DeleteActionId.ShouldNotBeNull();

                    modixContext.Set<ConfigurationActionEntity>().ShouldContain(x => x.Id == entity.DeleteActionId);
                    var deleteAction = modixContext.Set<ConfigurationActionEntity>().First(x => x.Id == entity.DeleteActionId);

                    deleteAction.GuildId.ShouldBe(entity.GuildId);
                    deleteAction.Type.ShouldBe(ConfigurationActionType.DesignatedRoleMappingDeleted);
                    deleteAction.Created.ShouldBeInRange(
                        DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1),
                        DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1));
                    deleteAction.CreatedById.ShouldBe(deletedById);
                    deleteAction.DesignatedRoleMappingId.ShouldBeNull();
                    deleteAction.DesignatedRoleMappingId.ShouldBe(entity.Id);
                    deleteAction.DesignatedRoleMappingId.ShouldBeNull();
                });

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsEnumerable()
                .Where(x => !designatedRoleMappingIds.Contains(x.Id) || DesignatedRoleMappings.Entities
                    .Any(y => (y.Id == x.Id) && (x.DeleteActionId == null)))
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsEnumerable()
                .Where(x => DesignatedRoleMappings.Entities
                    .Any(y => (y.DeleteActionId == x.Id) && designatedRoleMappingIds.Contains(y.Id)))
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(InvalidSearchCriteriaAndValidUserIdTestCases))]
        public async Task DeleteAsync_DesignatedRoleMappingsDoNotExist_DoesNotUpdateDesignatedRoleMappingsAndReturns0(DesignatedRoleMappingSearchCriteria criteria, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.DeleteAsync(criteria, deletedById);

            result.ShouldBe(0);

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedRoleMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedRoleMappingEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => x.DesignatedRoleMappingId != null)
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        #endregion DeleteAsync() Tests

        #region TryDeleteAsync() Tests

        [TestCaseSource(nameof(ActiveDesignatedRoleMappingAndValidUserIdTestCases))]
        public async Task TryDeleteAsync_DesignatedRoleMappingIsNotDeleted_UpdatesDesignatedRoleMappingAndReturnsTrue(long designatedRoleMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(designatedRoleMappingId, deletedById);

            result.ShouldBeTrue();

            modixContext.Set<DesignatedRoleMappingEntity>()
                .ShouldContain(x => x.Id == designatedRoleMappingId);
            var designatedRoleMapping = modixContext.Set<DesignatedRoleMappingEntity>()
                .First(x => x.Id == designatedRoleMappingId);

            var originalDesignatedRoleMapping = DesignatedRoleMappings.Entities
                .First(x => x.Id == designatedRoleMappingId);

            designatedRoleMapping.GuildId.ShouldBe(originalDesignatedRoleMapping.GuildId);
            designatedRoleMapping.RoleId.ShouldBe(originalDesignatedRoleMapping.RoleId);
            designatedRoleMapping.Type.ShouldBe(originalDesignatedRoleMapping.Type);
            designatedRoleMapping.CreateActionId.ShouldBe(originalDesignatedRoleMapping.CreateActionId);
            designatedRoleMapping.DeleteActionId.ShouldNotBeNull();

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedRoleMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedRoleMappingEntity>()
                .Where(x => x.Id != designatedRoleMappingId)
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .ShouldContain(x => x.Id == designatedRoleMapping.DeleteActionId);
            var deleteAction = modixContext.Set<ConfigurationActionEntity>()
                .First(x => x.Id == designatedRoleMapping.DeleteActionId);

            deleteAction.GuildId.ShouldBe(designatedRoleMapping.GuildId);
            deleteAction.Type.ShouldBe(ConfigurationActionType.DesignatedRoleMappingDeleted);
            deleteAction.Created.ShouldBeInRange(
                DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1),
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1));
            deleteAction.CreatedById.ShouldBe(deletedById);
            deleteAction.DesignatedChannelMappingId.ShouldBeNull();
            deleteAction.DesignatedRoleMappingId.ShouldNotBeNull();
            deleteAction.DesignatedRoleMappingId.ShouldBe(designatedRoleMapping.Id);

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.DesignatedRoleMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .Where(x => x.Id != deleteAction.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DeletedDesignatedRoleMappingAndValidUserIdTestCases))]
        [TestCaseSource(nameof(InvalidDesignatedRoleMappingAndValidUserIdTestCases))]
        public async Task TryDeleteAsync_DesignatedRoleMappingIsDeleted_DoesNotUpdateDesignatedRoleMappingsAndReturnsFalse(long designatedRoleMappingId, ulong deletedById)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(designatedRoleMappingId, deletedById);

            result.ShouldBeFalse();

            modixContext.Set<DesignatedRoleMappingEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(DesignatedRoleMappings.Entities
                    .Select(x => x.Id));

            modixContext.Set<DesignatedRoleMappingEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<ConfigurationActionEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(ConfigurationActions.Entities
                    .Where(x => !(x.DesignatedRoleMappingId is null))
                    .Select(x => x.Id));

            modixContext.Set<ConfigurationActionEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryDeleteAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> DesignatedRoleMappingCreationTestCases
            = DesignatedRoleMappings.Creations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.GuildId}, {x.RoleId}, {x.Type})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaTestCases
            = DesignatedRoleMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaAndResultIdsTestCases
            = DesignatedRoleMappings.Searches
                .Where(x => x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria, x.resultIds)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> ValidSearchCriteriaAndValidUserIdAndResultIdsTestCases
            = DesignatedRoleMappings.Searches
                .Where(x => x.resultIds.Any())
                .SelectMany(x => Users.Entities
                    .Where(y => DesignatedRoleMappings.Entities
                        .Where(z => x.resultIds.Contains(z.Id))
                        .Select(z => z.GuildId)
                        .Distinct()
                        .All(z => GuildUsers.Entities
                            .Any(w => (w.UserId == y.Id) && (w.GuildId == z))))
                    .Select(y => new TestCaseData(x.criteria, y.Id, x.resultIds)
                        .SetName($"{{m}}(\"{x.name}\", {y.Id})")));

        public static readonly IEnumerable<TestCaseData> InvalidSearchCriteriaTestCases
            = DesignatedRoleMappings.Searches
                .Where(x => !x.resultIds.Any())
                .Select(x => new TestCaseData(x.criteria)
                    .SetName($"{{m}}({x.name})"));

        public static readonly IEnumerable<TestCaseData> InvalidSearchCriteriaAndValidUserIdTestCases
            = DesignatedRoleMappings.Searches
                .Where(x => !x.resultIds.Any())
                .SelectMany(x => Users.Entities
                    .Select(y => new TestCaseData(x.criteria, y.Id)
                        .SetName($"{{m}}(\"{x.name}\", {y.Id})")));

        public static readonly IEnumerable<TestCaseData> ActiveDesignatedRoleMappingAndValidUserIdTestCases
            = DesignatedRoleMappings.Entities
                .Where(x => x.DeleteActionId is null)
                .SelectMany(x => GuildUsers.Entities
                    .Where(y => y.GuildId == x.GuildId)
                    .Select(y => new TestCaseData(x.Id, y.UserId)));

        public static readonly IEnumerable<TestCaseData> DeletedDesignatedRoleMappingAndValidUserIdTestCases
            = DesignatedRoleMappings.Entities
                .Where(x => !(x.DeleteActionId is null))
                .SelectMany(x => GuildUsers.Entities
                    .Where(y => y.GuildId == x.GuildId)
                    .Select(y => new TestCaseData(x.Id, y.UserId)));

        public static readonly IEnumerable<TestCaseData> InvalidDesignatedRoleMappingAndValidUserIdTestCases
            = Enumerable.Empty<long>()
                .Append(DesignatedRoleMappings.Entities
                    .Select(x => x.Id)
                    .Max() + 1)
                .SelectMany(x => Users.Entities
                    .Select(y => new TestCaseData(x, y.Id)));

        #endregion Test Data
    }
}
