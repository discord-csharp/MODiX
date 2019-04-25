using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;
using Modix.Data.Test.TestData.Emoji;

using NSubstitute;

using NUnit.Framework;

using Shouldly;

namespace Modix.Data.Test.Repositories
{
    internal class EmojiRepositoryTests
    {
        #region Test Context

        private static (ModixContext, EmojiRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Emoji.AddRange(Emoji.Entities.Clone());
            });

            var uut = new EmojiRepository(modixContext);

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
            (_, var uut) = BuildTestContext();

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
            (_, var uut) = BuildTestContext();

            var result = uut.BeginMaintainTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

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

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_DoesNotUpdateEmojisAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null));

            modixContext.Emoji.Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(EmojiCreationTestCases))]
        public async Task CreateAsync_DataIsNotNull_InsertsEmoji(EmojiCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var id = await uut.CreateAsync(data);

            modixContext.Emoji.ShouldContain(x => x.Id == id);
            var emoji = modixContext.Emoji.First(x => x.Id == id);

            emoji.GuildId.ShouldBe(data.GuildId);
            emoji.ChannelId.ShouldBe(data.ChannelId);
            emoji.MessageId.ShouldBe(data.MessageId);
            emoji.UserId.ShouldBe(data.UserId);
            emoji.EmojiId.ShouldBe(data.EmojiId);
            emoji.EmojiName.ShouldBe(data.EmojiName);
            emoji.IsAnimated.ShouldBe(data.IsAnimated);
            emoji.Timestamp.ShouldBeInRange(
                DateTimeOffset.Now - TimeSpan.FromSeconds(1),
                DateTimeOffset.Now + TimeSpan.FromSeconds(1));
            emoji.UsageType.ShouldBe(data.UsageType);

            modixContext.Emoji.Where(x => x.Id != emoji.Id).Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.Where(x => x.Id != emoji.Id).EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region CreateMultipleAsync() Tests

        [Test]
        public async Task CreateMultipleAsync_DataIsNull_DoesNotUpdateEmojisAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateMultipleAsync(null, 1));

            modixContext.Emoji.Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(EmojiCreationTestCases))]
        public async Task CreateMultipleAsync_CountIsZero_DoesNotUpdateEmojis(EmojiCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateMultipleAsync(data, 0);

            modixContext.Emoji.Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(EmojiCreationTestCases))]
        public async Task CreateMultipleAsync_CountIsNegative_DoesNotUpdateEmojis(EmojiCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateMultipleAsync(data, -1);

            modixContext.Emoji.Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(EmojiCreationTestCases))]
        public async Task CreateMultipleAsync_DataIsNotNull_InsertsEmojis(EmojiCreationData data)
        {
            const int RecordCount = 100;

            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateMultipleAsync(data, RecordCount);

            var emojis = modixContext.Emoji
                .OrderByDescending(x => x.Id)
                .Take(RecordCount)
                .ToArray();

            foreach (var emoji in emojis)
            {
                emoji.GuildId.ShouldBe(data.GuildId);
                emoji.ChannelId.ShouldBe(data.ChannelId);
                emoji.MessageId.ShouldBe(data.MessageId);
                emoji.UserId.ShouldBe(data.UserId);
                emoji.EmojiId.ShouldBe(data.EmojiId);
                emoji.EmojiName.ShouldBe(data.EmojiName);
                emoji.IsAnimated.ShouldBe(data.IsAnimated);
                emoji.Timestamp.ShouldBeInRange(
                    DateTimeOffset.Now - TimeSpan.FromSeconds(1),
                    DateTimeOffset.Now + TimeSpan.FromSeconds(1));
                emoji.UsageType.ShouldBe(data.UsageType);
            }

            modixContext.Emoji.Where(x => !emojis.Select(y => y.Id).Contains(x.Id)).Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.Where(x => !emojis.Select(y => y.Id).Contains(x.Id)).EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        #endregion CreateMultipleAsync() Tests

        #region DeleteAsync() Tests

        [TestCaseSource(nameof(ExceptionDeleteTestCases))]
        public async Task DeleteAsync_CriteriaIsInvalid_Throws(EmojiSearchCriteria criteria, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.DeleteAsync(criteria), exceptionType);
        }

        [TestCaseSource(nameof(NonexistentDeleteTestCases))]
        public async Task DeleteAsync_Nonexistent_DoesNotDeleteEmojis(EmojiSearchCriteria criteria)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.DeleteAsync(criteria);

            modixContext.Emoji.Count().ShouldBe(Emoji.Entities.Count());
            modixContext.Emoji.Select(x => x.Id).ShouldBe(Emoji.Entities.Select(x => x.Id));
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());
        }

        [TestCaseSource(nameof(ValidDeleteTestCases))]
        public async Task DeleteAsync_ValidCriteria_DeletesEmojis(EmojiSearchCriteria criteria, int deletedCount)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.DeleteAsync(criteria);

            modixContext.Emoji.Count().ShouldBe(Emoji.Entities.Count() - deletedCount);
            modixContext.Emoji.EachShould(x => x.ShouldNotHaveChanged());
        }

        #endregion DeleteAsync() Tests

        #region Single GetEmojiStatsAsync() Tests

        [TestCaseSource(nameof(ExceptionSingleEmojiStatsTestCases))]
        public async Task SingleGetEmojiStatsAsync_CriteriaIsInvalid_Throws(ulong guildId, EphemeralEmoji emoji, TimeSpan? dateFilter, Type exceptionType)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync(() => uut.GetEmojiStatsAsync(guildId, emoji, dateFilter), exceptionType);
        }

        #endregion Single GetEmojiStatsAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> EmojiCreationTestCases
            = Creations.ValidCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.EmojiId}, {x.EmojiName}, {x.UsageType})"));

        public static readonly IEnumerable<TestCaseData> ExceptionDeleteTestCases
            = Deletions.ExceptionDeletions
                .Select(x => new TestCaseData(x.Criteria, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentDeleteTestCases
            = Deletions.NonexistentDeletions
                .Select(x => new TestCaseData(x.Criteria)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ValidDeleteTestCases
            = Deletions.ValidDeletions
                .Select(x => new TestCaseData(x.Criteria, x.DeletedCount)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ExceptionSingleEmojiStatsTestCases
            = SingleEmoji.Exceptions
                .Select(x => new TestCaseData(x.GuildId, x.Emoji, x.DateFilter, x.ExceptionType)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> NonexistentSingleEmojiStatsTestCases
            = SingleEmoji.Nonexistent
                .Select(x => new TestCaseData(x.GuildId, x.Emoji, x.DateFilter)
                    .SetName($"{{m}}({x.TestName})"));

        public static readonly IEnumerable<TestCaseData> ValidSingleEmojiStatsTestCases
            = SingleEmoji.Valid
                .Select(x => new TestCaseData(x.GuildId, x.Emoji, x.DateFilter, x.Result)
                    .SetName($"{{m}}({x.TestName})"));

        #endregion Test Data
    }
}
