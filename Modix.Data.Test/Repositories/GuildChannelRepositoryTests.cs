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
    [TestFixture]
    public class GuildChannelRepositoryTests
    {
        #region Test Context

        private static (ModixContext, GuildChannelRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.GuildChannels.AddRange(GuildChannels.Entities.Clone());
            });

            var uut = new GuildChannelRepository(modixContext);

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

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_DoesNotUpdateGuildChannelsAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null));

            modixContext.GuildChannels.Select(x => x.ChannelId).ShouldBe(GuildChannels.Entities.Select(x => x.ChannelId));
            modixContext.GuildChannels.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildChannelCreationTestCases))]
        public async Task CreateAsync_GuildChannelDoesNotExist_InsertsGuildChannel(GuildChannelCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data);

            modixContext.GuildChannels.ShouldContain(x => x.ChannelId == data.ChannelId);
            var role = modixContext.GuildChannels.First(x => x.ChannelId == data.ChannelId);

            role.GuildId.ShouldBe(data.GuildId);
            role.Name.ShouldBe(data.Name);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildChannelCreationTestCases))]
        public async Task CreateAsync_GuildChannelExists_DoesNotUpdateGuildChannelsAndThrowsException(GuildChannelCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<InvalidOperationException>(uut.CreateAsync(data));

            modixContext.GuildChannels.Select(x => x.ChannelId).ShouldBe(GuildChannels.Entities.Select(x => x.ChannelId));
            modixContext.GuildChannels.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region TryUpateAsync() Tests

        [Test]
        public async Task TryUpdateAsync_UpdateActionIsNull_DoesNotUpdateGuildChannelsAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await uut.TryUpdateAsync(1, null));

            modixContext.GuildChannels.Select(x => x.ChannelId).ShouldBe(GuildChannels.Entities.Select(x => x.ChannelId));
            modixContext.GuildChannels.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildChannelIds))]
        public async Task TryUpdateAsync_GuildChannelExists_UpdatesGuildChannelAndReturnsTrue(ulong roleId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var guildChannel = modixContext.GuildChannels.Single(x => x.ChannelId == roleId);

            var mutatedData = new GuildChannelMutationData()
            {
                Name = "UpdatedChannel"
            };

            var result = await uut.TryUpdateAsync(roleId, data =>
            {
                data.Name.ShouldBe(guildChannel.Name);

                data.Name = mutatedData.Name;
            });

            result.ShouldBeTrue();

            guildChannel.Name.ShouldBe(mutatedData.Name);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildChannelIds))]
        public async Task TryUpdateAsync_GuildChannelDoesNotExist_DoesNotUpdateGuildChannelsAndReturnsFalse(ulong roleId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var updateAction = Substitute.For<Action<GuildChannelMutationData>>();

            var result = await uut.TryUpdateAsync(roleId, updateAction);

            result.ShouldBeFalse();

            updateAction.ShouldNotHaveReceived()
                .Invoke(Arg.Any<GuildChannelMutationData>());

            modixContext.GuildChannels.Select(x => x.ChannelId).ShouldBe(GuildChannels.Entities.Select(x => x.ChannelId));
            modixContext.GuildChannels.EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryUpateAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> NewGuildChannelCreationTestCases
            = GuildChannels.NewCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.ChannelId})"));

        public static readonly IEnumerable<TestCaseData> ExistingGuildChannelCreationTestCases
            = GuildChannels.ExistingCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.ChannelId})"));

        public static readonly IEnumerable<ulong> NewGuildChannelIds
            = GuildChannels.NewCreations
                .Select(x => x.ChannelId);

        public static readonly IEnumerable<ulong> ExistingGuildChannelIds
            = GuildChannels.ExistingCreations
                .Select(x => x.ChannelId);

        #endregion Test Data
    }
}
