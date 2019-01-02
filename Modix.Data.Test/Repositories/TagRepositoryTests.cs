using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Modix.Data.Models.Tags;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;
using Modix.Data.Test.TestData.Tags;

using NSubstitute;

using NUnit.Framework;

using Shouldly;

namespace Modix.Data.Test.Repositories
{
    internal class TagRepositoryTests
    {
        #region Test Context

        private static (ModixContext, TagRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Users.AddRange(Users.Entities.Clone());
                x.GuildUsers.AddRange(GuildUsers.Entities.Clone());
                x.Tags.AddRange(Tags.Entities.Clone());
                x.TagActions.AddRange(TagActions.Entities.Clone());
            });

            var uut = new TagRepository(modixContext);

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

        #region BeginMaintainTransactionAsync() Tests

        [Test]
        [NonParallelizable]
        public async Task BeginMaintainTransactionAsync_MaintainTransactionIsInProgress_WaitsForCompletion()
        {
            (var modixContext, var uut) = BuildTestContext();

            var existingTransaction = await uut.BeginMaintainTransactionAsync();

            var result = uut.BeginMaintainTransactionAsync();

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginMaintainTransactionAsync_MaintainTransactionIsNotInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = uut.BeginMaintainTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginMaintainTransactionAsync_UseTransactionIsInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var deleteTransaction = await uut.BeginUseTransactionAsync();

            var result = uut.BeginMaintainTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            deleteTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginMaintainTransactionAsync_Always_TransactionIsForContextDatabase()
        {
            (var modixContext, var uut) = BuildTestContext();

            var database = Substitute.ForPartsOf<DatabaseFacade>(modixContext);
            modixContext.Database.Returns(database);

            using (var transaction = await uut.BeginMaintainTransactionAsync())
            { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginMaintainTransactionAsync() Tests

        #region BeginUseTransactionAsync() Tests

        [Test]
        [NonParallelizable]
        public async Task BeginUseTransactionAsync_UseTransactionIsInProgress_WaitsForCompletion()
        {
            (var modixContext, var uut) = BuildTestContext();

            var existingTransaction = await uut.BeginUseTransactionAsync();

            var result = uut.BeginUseTransactionAsync();

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginUseTransactionAsync_UseTransactionIsNotInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = uut.BeginUseTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginUseTransactionAsync_MaintainTransactionIsInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var createTransaction = await uut.BeginMaintainTransactionAsync();

            var result = uut.BeginUseTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            createTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginUseTransactionAsync_Always_TransactionIsForContextDatabase()
        {
            (var modixContext, var uut) = BuildTestContext();

            var database = Substitute.ForPartsOf<DatabaseFacade>(modixContext);
            modixContext.Database.Returns(database);

            using (var transaction = await uut.BeginUseTransactionAsync())
            { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginDeleteTransactionAsync() Tests

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_DoesNotUpdateTagsAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null));

            modixContext.Tags.Select(x => x.Id).ShouldBe(Tags.Entities.Select(x => x.Id));
            modixContext.Tags.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(TagCreationTestCases))]
        public async Task CreateAsync_DataIsNotNull_InsertsTag(TagCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var id = await uut.CreateAsync(data);

            modixContext.Tags.ShouldContain(x => x.Id == id);
            var tag = modixContext.Tags.First(x => x.Id == id);

            tag.GuildId.ShouldBe(data.GuildId);
            tag.CreateAction.CreatedById.ShouldBe(data.CreatedById);
            tag.Name.ShouldBe(data.Name);
            tag.Content.ShouldBe(data.Content);
            tag.Uses.ShouldBe(data.Uses);
            tag.CreateActionId.ShouldNotBeNull();
            tag.DeleteActionId.ShouldBeNull();

            modixContext.Tags.Where(x => x.Id != tag.Id).Select(x => x.Id).ShouldBe(Tags.Entities.Select(x => x.Id));
            modixContext.Tags.Where(x => x.Id != tag.Id).EachShould(x => x.ShouldNotHaveChanged());

            modixContext.TagActions.ShouldContain(x => x.Id == tag.CreateActionId);
            var createAction = modixContext.TagActions.First(x => x.Id == tag.CreateActionId);

            createAction.GuildId.ShouldBe(data.GuildId);
            createAction.Type.ShouldBe(TagActionType.TagCreated);
            createAction.Created.ShouldBeInRange(
                DateTimeOffset.Now - TimeSpan.FromSeconds(1),
                DateTimeOffset.Now + TimeSpan.FromSeconds(1));
            createAction.NewTagId.ShouldBe(tag.Id);
            createAction.OldTagId.ShouldBeNull();

            modixContext.TagActions.Where(x => x.Id != createAction.Id).Select(x => x.Id).ShouldBe(TagActions.Entities.Select(x => x.Id));
            modixContext.TagActions.Where(x => x.Id != createAction.Id).EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(2)
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region ReadSummaryAsync() Tests

        [TestCaseSource(nameof(ValidReadTestCases))]
        public async Task ReadSummaryAsync_TagExists_ReturnsMatchingSummary(ulong guildId, string name, long resultId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadSummaryAsync(guildId, name);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(resultId);
            result.ShouldMatchTestData();
        }

        [TestCaseSource(nameof(NonexistentReadTestCases))]
        public async Task ReadSummaryAsync_TagDoesNotExist_ReturnsNull(ulong guildId, string name)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadSummaryAsync(guildId, name);

            result.ShouldBeNull();
        }

        [TestCaseSource(nameof(ExceptionReadTestCases))]
        public async Task ReadSummaryAsync_CriteriaIsInvalid_Throws(ulong guildId, string name, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.ReadSummaryAsync(guildId, name), exceptionType);
        }

        #endregion ReadSummaryAsync() Tests

        #region TryIncrementUsesAsync() Tests

        [TestCaseSource(nameof(ValidIncrementTestCases))]
        public async Task TryIncrementUsesAsync_TagExists_IncrementsUsesAndReturnsTrue(ulong guildId, string name, uint resultantUses)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryIncrementUsesAsync(guildId, name);
            result.ShouldBeTrue();

            var summary = await uut.ReadSummaryAsync(guildId, name);
            summary.Uses.ShouldBe(resultantUses);
        }

        [TestCaseSource(nameof(NonexistentIncrementTestCases))]
        public async Task TryIncrementUsesAsync_TagDoesNotExist_ReturnsFalse(ulong guildId, string name)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryIncrementUsesAsync(guildId, name);
            result.ShouldBeFalse();
        }

        [TestCaseSource(nameof(ExceptionIncrementTestCases))]
        public async Task TryIncrementUsesAsync_CriteriaIsInvalid_Throws(ulong guildId, string name, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.TryIncrementUsesAsync(guildId, name), exceptionType);
        }

        #endregion TryIncrementUsesAsync() Tests

        #region TryModifyAsync() Tests

        [TestCaseSource(nameof(ValidModifyTestCases))]
        public async Task TryModifyAsync_TagExists_ModifesTagAndReturnsTrue(
            ulong guildId, string name, ulong modifiedByUserId, Action<TagMutationData> modifyAction, Func<TagSummary, TagSummary, bool> predicate)
        {
            (var modixContext, var uut) = BuildTestContext();

            var oldSummary = await uut.ReadSummaryAsync(guildId, name);
            oldSummary.ShouldNotBeNull();

            var result = await uut.TryModifyAsync(guildId, name, modifiedByUserId, modifyAction);
            result.ShouldBeTrue();

            var newSummary = await uut.ReadSummaryAsync(guildId, name);
            newSummary.ShouldNotBeNull();
            predicate(newSummary, oldSummary).ShouldBeTrue();
        }

        [TestCaseSource(nameof(NonexistentModifyTestCases))]
        public async Task TryModifyAsync_TagDoesNotExist_ReturnsFalse(ulong guildId, string name, ulong modifiedByUserId, Action<TagMutationData> modifyAction)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryModifyAsync(guildId, name, modifiedByUserId, modifyAction);
            result.ShouldBeFalse();
        }

        [TestCaseSource(nameof(ExceptionModifyTestCases))]
        public async Task TryModifyAsync_CriteriaIsInvalid_Throws(ulong guildId, string name, ulong modifiedByUserId, Action<TagMutationData> modifyAction, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.TryModifyAsync(guildId, name, modifiedByUserId, modifyAction), exceptionType);
        }

        #endregion TryModifyAsync() Tests

        #region TryDeleteAsync() Tests

        [TestCaseSource(nameof(ValidDeleteTestCases))]
        public async Task TryDeleteAsync_TagExists_DeletesTagAndReturnsTrue(ulong guildId, string name, ulong deletedByUserId)
        {
            (var modixContext, var uut) = BuildTestContext();
            
            var result = await uut.TryDeleteAsync(guildId, name, deletedByUserId);
            result.ShouldBeTrue();

            var summary = await uut.ReadSummaryAsync(guildId, name);
            summary.ShouldBeNull();
        }

        [TestCaseSource(nameof(NonexistentDeleteTestCases))]
        public async Task TryDeleteAsync_TagDoesNotExist_ReturnsFalse(ulong guildId, string name, ulong deletedByUserId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.TryDeleteAsync(guildId, name, deletedByUserId);
            result.ShouldBeFalse();
        }

        [TestCaseSource(nameof(ExceptionDeleteTestCases))]
        public async Task TryDeleteAsync_CriteriaIsInvalid_Throws(ulong guildId, string name, ulong deletedByUserId, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.TryDeleteAsync(guildId, name, deletedByUserId), exceptionType);
        }

        #endregion TryDeleteAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> TagCreationTestCases
            = Creations.ValidCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.GuildId}, {x.Name})"));

        public static readonly IEnumerable<TestCaseData> ValidReadTestCases
            = Reads.ValidReads
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ResultId)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentReadTestCases
            = Reads.NonexistentReads
                .Select(x => new TestCaseData(x.GuildId, x.TagName)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ExceptionReadTestCases
            = Reads.ExceptionReads
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ValidIncrementTestCases
            = Increments.ValidIncrements
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ResultantUses)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentIncrementTestCases
            = Increments.NonexistentIncrements
                .Select(x => new TestCaseData(x.GuildId, x.TagName)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ExceptionIncrementTestCases
            = Increments.ExceptionIncrements
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ValidModifyTestCases
            = Modifications.ValidModifications
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ModifiedByUserId, x.ModifyAction, x.Predicate)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentModifyTestCases
            = Modifications.NonexistentModifications
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ModifiedByUserId, x.ModifyAction)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ExceptionModifyTestCases
            = Modifications.ExceptionModifications
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.ModifiedByUserId, x.ModifyAction, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ValidDeleteTestCases
            = Deletions.ValidDeletions
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.DeletedByUserId)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentDeleteTestCases
            = Deletions.NonexistentDeletions
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.DeletedByUserId)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ExceptionDeleteTestCases
            = Deletions.ExceptionDeletions
                .Select(x => new TestCaseData(x.GuildId, x.TagName, x.DeletedByUserId, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        #endregion Test Data
    }
}
