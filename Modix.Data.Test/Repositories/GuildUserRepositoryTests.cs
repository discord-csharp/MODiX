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
    public class GuildUserRepositoryTests
    {
        #region Test Context

        private static (ModixContext, GuildUserRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<UserEntity>().AddRange(Users.Entities.Clone());
                x.Set<GuildUserEntity>().AddRange(GuildUsers.Entities.Clone());
            });

            var uut = new GuildUserRepository(modixContext);

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

            var existingTransaction = await uut.BeginCreateTransactionAsync(default);

            var result = uut.BeginCreateTransactionAsync(default);

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        [NonParallelizable]
        public async Task BeginCreateTransactionAsync_CreateTransactionIsNotInProgress_ReturnsImmediately()
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = uut.BeginCreateTransactionAsync(default);

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

            using (var transaction = await uut.BeginCreateTransactionAsync(default)) { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginCreateTransactionAsync() Tests

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_DoesNotUpdateUsersAndGuildUsersAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await uut.CreateAsync(null!, default));

            modixContext.Set<GuildUserEntity>().AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewUserCreationTestCases))]
        public async Task CreateAsync_UserDoesNotExist_InsertsUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data, default);

            modixContext.Set<UserEntity>().ShouldContain(x => x.Id == data.UserId);
            var user = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != user.Id)
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingUserCreationTestCases))]
        public async Task CreateAsync_UserExists_UpdatesUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var userCount = modixContext.Set<UserEntity>().Count();

            await uut.CreateAsync(data, default);

            modixContext.Set<UserEntity>().Count().ShouldBe(userCount);
            var user = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingUserCreationTestCases))]
        public async Task CreateAsync_UserExistsAndDataUsernameIsNull_DoesNotUpdateUsername(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            data.Username = null!;

            var userCount = modixContext.Set<UserEntity>().Count();
            var previousUser = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            await uut.CreateAsync(data, default);

            modixContext.Set<UserEntity>().Count().ShouldBe(userCount);
            var user = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            user.Username.ShouldBe(previousUser.Username);

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingUserCreationTestCases))]
        public async Task CreateAsync_UserExistsAndDataDiscriminatorIsNull_DoesNotUpdateDiscriminator(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            data.Discriminator = null!;

            var userCount = modixContext.Set<UserEntity>().Count();
            var previousUser = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            await uut.CreateAsync(data, default);

            modixContext.Set<UserEntity>().Count().ShouldBe(userCount);
            var user = modixContext.Set<UserEntity>().First(x => x.Id == data.UserId);

            user.Discriminator.ShouldBe(previousUser.Discriminator);

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildUserCreationTestCases))]
        public async Task CreateAsync_GuildUserDoesNotExist_InsertsGuildUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data, default);

            modixContext.Set<GuildUserEntity>()
                .ShouldContain(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));
            var guildUser = modixContext.Set<GuildUserEntity>()
                .First(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));

            guildUser.Nickname.ShouldBe(data.Nickname);
            guildUser.FirstSeen.ShouldBe(data.FirstSeen);
            guildUser.LastSeen.ShouldBe(data.LastSeen);

            modixContext.Set<GuildUserEntity>()
                .AsEnumerable()
                .Where(x => (x.GuildId != guildUser.GuildId) || (x.UserId != guildUser.UserId))
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .Where(x => (x.UserId != guildUser.UserId) || (x.GuildId != guildUser.GuildId))
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildUserCreationTestCases))]
        public async Task CreateAsync_GuildUserExists_DoesNotUpdateUsersAndGuildUsersAndThrowsException(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<InvalidOperationException>(uut.CreateAsync(data, default));

            modixContext.Set<GuildUserEntity>().AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region ReadSummaryAsync() Tests

        [TestCaseSource(nameof(ExistingGuildUserIds))]
        public async Task ReadSummaryAsync_GuildUserExists_ReturnsGuildUser(ulong userId, ulong guildId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadSummaryAsync(userId, guildId);

            result.ShouldNotBeNull();
            result.UserId.ShouldBe(userId);
            result.GuildId.ShouldBe(guildId);
            result.ShouldMatchTestData();
        }

        [TestCaseSource(nameof(NewGuildUserIds))]
        public async Task ReadSummaryAsync_GuildUserDoesNotExist_ReturnsNull(ulong userId, ulong guildId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadSummaryAsync(userId, guildId);

            result.ShouldBeNull();
        }

        #endregion ReadSummaryAsync() Tests

        #region TryUpdateAsync() Tests

        [Test]
        public async Task TryUpdateAsync_UpdateActionIsNull_DoesNotUpdateUsersAndGuildUsersAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await uut.TryUpdateAsync(1, 1, null!, default));

            modixContext.Set<GuildUserEntity>().AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildUserIds))]
        public async Task TryUpdateAsync_GuildUserExists_UpdatesGuildUsersAndReturnsTrue(ulong userId, ulong guildId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var user = modixContext.Set<UserEntity>().Single(x => x.Id == userId);
            var guildUser = modixContext.Set<GuildUserEntity>().Single(x => (x.UserId == userId) && (x.GuildId == guildId));

            var mutatedData = new GuildUserMutationData()
            {
                Username = "UpdatedUsername",
                Discriminator = "UpdatedDiscriminator",
                Nickname = "UpdatedNickname",
                LastSeen = DateTimeOffset.UtcNow
            };

            var result = await uut.TryUpdateAsync(userId, guildId, data =>
            {
                data.Username.ShouldBe(user.Username);
                data.Discriminator.ShouldBe(user.Discriminator);
                data.Nickname.ShouldBe(guildUser.Nickname);
                data.LastSeen.ShouldBe(guildUser.LastSeen);

                data.Username = mutatedData.Username;
                data.Discriminator = mutatedData.Discriminator;
                data.Nickname = mutatedData.Nickname;
                data.LastSeen = mutatedData.LastSeen;
            }, default);

            result.ShouldBeTrue();

            user.Username.ShouldBe(mutatedData.Username);
            user.Discriminator.ShouldBe(mutatedData.Discriminator);
            guildUser.Nickname.ShouldBe(mutatedData.Nickname);
            guildUser.LastSeen.ShouldBe(mutatedData.LastSeen);

            modixContext.Set<GuildUserEntity>().AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .Where(x => (x.UserId != userId) || (x.GuildId != guildId))
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .Where(x => x.Id != userId)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildUserIds))]
        public async Task TryUpdateAsync_GuildUserDoesNotExist_DoesNotUpdateUsersAndGuildUsersAndReturnsFalse(ulong userId, ulong guildId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var updateAction = Substitute.For<Action<GuildUserMutationData>>();

            var result = await uut.TryUpdateAsync(userId, guildId, updateAction, default);

            result.ShouldBeFalse();

            updateAction.ShouldNotHaveReceived()
                .Invoke(Arg.Any<GuildUserMutationData>());

            modixContext.Set<GuildUserEntity>().AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.Set<GuildUserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Set<UserEntity>()
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Set<UserEntity>()
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryUpdateAsync() Tests

        #region Test Data

        public static readonly IEnumerable<TestCaseData> NewUserCreationTestCases
            = Users.NewCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.UserId}, {x.GuildId})"));

        public static readonly IEnumerable<TestCaseData> ExistingUserCreationTestCases
            = Users.ExistingCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.UserId}, {x.GuildId})"));

        public static readonly IEnumerable<TestCaseData> NewGuildUserCreationTestCases
            = GuildUsers.NewCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.UserId}, {x.GuildId})"));

        public static readonly IEnumerable<TestCaseData> ExistingGuildUserCreationTestCases
            = GuildUsers.ExistingCreations
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.UserId}, {x.GuildId})"));

        public static readonly IEnumerable<ulong[]> NewGuildUserIds
            = GuildUsers.NewCreations
                .Select(x => new[] { x.UserId, x.GuildId });

        public static readonly IEnumerable<ulong[]> ExistingGuildUserIds
            = GuildUsers.ExistingCreations
                .Select(x => new[] { x.UserId, x.GuildId });

        #endregion Test Data
    }
}
