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
    public class GuildRoleRepositoryTests
    {
        #region Test Context

        private static (ModixContext, GuildRoleRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<GuildRoleEntity>().AddRange(GuildRoles.Entities.Clone());
            });

            var uut = new GuildRoleRepository(modixContext);

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
        public async Task CreateAsync_DataIsNull_DoesNotUpdateGuildRolesAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(uut.CreateAsync(null!));

            modixContext.Set<GuildRoleEntity>()
                .AsQueryable()
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildRoleCreationTestCases))]
        public async Task CreateAsync_GuildRoleDoesNotExist_InsertsGuildRole(GuildRoleCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data);

            modixContext.Set<GuildRoleEntity>().ShouldContain(x => x.RoleId == data.RoleId);
            var role = modixContext.Set<GuildRoleEntity>().First(x => x.RoleId == data.RoleId);

            role.GuildId.ShouldBe(data.GuildId);
            role.Name.ShouldBe(data.Name);
            role.Position.ShouldBe(data.Position);

            modixContext.Set<GuildRoleEntity>()
                .Where(x => x.RoleId != role.RoleId)
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .Where(x => x.RoleId != role.RoleId)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildRoleCreationTestCases))]
        public async Task CreateAsync_GuildRoleExists_DoesNotUpdateGuildRolesAndThrowsException(GuildRoleCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<InvalidOperationException>(uut.CreateAsync(data));

            modixContext.Set<GuildRoleEntity>()
                .AsQueryable()
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region TryUpateAsync() Tests

        [Test]
        public async Task TryUpdateAsync_UpdateActionIsNull_DoesNotUpdateGuildRolesAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await uut.TryUpdateAsync(1, null!));

            modixContext.Set<GuildRoleEntity>()
                .AsQueryable()
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildRoleIds))]
        public async Task TryUpdateAsync_GuildRoleExists_UpdatesGuildRolesAndReturnsTrue(ulong roleId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var guildRole = modixContext.Set<GuildRoleEntity>().Single(x => x.RoleId == roleId);

            var mutatedData = new GuildRoleMutationData()
            {
                Name = "UpdatedRole",
                Position = 20
            };

            var result = await uut.TryUpdateAsync(roleId, data =>
            {
                data.Name.ShouldBe(guildRole.Name);
                data.Position.ShouldBe(guildRole.Position);

                data.Name = mutatedData.Name;
                data.Position = mutatedData.Position;
            });

            result.ShouldBeTrue();

            guildRole.Name.ShouldBe(mutatedData.Name);
            guildRole.Position.ShouldBe(mutatedData.Position);

            modixContext.Set<GuildRoleEntity>()
                .AsQueryable()
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .Where(x => x.RoleId != roleId)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildRoleIds))]
        public async Task TryUpdateAsync_GuildRoleDoesNotExist_DoesNotUpdateGuildRolesAndReturnsFalse(ulong roleId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var updateAction = Substitute.For<Action<GuildRoleMutationData>>();

            var result = await uut.TryUpdateAsync(roleId, updateAction);

            result.ShouldBeFalse();

            updateAction.ShouldNotHaveReceived()
                .Invoke(Arg.Any<GuildRoleMutationData>());

            modixContext.Set<GuildRoleEntity>()
                .AsQueryable()
                .Select(x => x.RoleId)
                .ShouldBe(GuildRoles.Entities
                    .Select(x => x.RoleId));

            modixContext.Set<GuildRoleEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryUpateAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> NewGuildRoleCreationTestCases
            = GuildRoles.NewCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.RoleId})"));

        public static readonly IEnumerable<TestCaseData> ExistingGuildRoleCreationTestCases
            = GuildRoles.ExistingCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.RoleId})"));

        public static readonly IEnumerable<ulong> NewGuildRoleIds
            = GuildRoles.NewCreations
                .Select(x => x.RoleId);

        public static readonly IEnumerable<ulong> ExistingGuildRoleIds
            = GuildRoles.ExistingCreations
                .Select(x => x.RoleId);

        #endregion Test Data
    }
}
