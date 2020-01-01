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
                x.Users.AddRange(Users.Entities.Clone());
                x.GuildUsers.AddRange(GuildUsers.Entities.Clone());
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
        public async Task CreateAsync_DataIsNull_DoesNotUpdateUsersAndGuildUsersAndThrowsException()
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await uut.CreateAsync(null!));

            modixContext.GuildUsers.AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewUserCreationTestCases))]
        public async Task CreateAsync_UserDoesNotExist_InsertsUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data);

            modixContext.Users.ShouldContain(x => x.Id == data.UserId);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            modixContext.Users
                .Where(x => x.Id != user.Id)
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingUserCreationTestCases))]
        public async Task CreateAsync_UserExists_UpdatesUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            var userCount = modixContext.Users.Count();

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
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

            var userCount = modixContext.Users.Count();
            var previousUser = modixContext.Users.First(x => x.Id == data.UserId);

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(previousUser.Username);

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
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

            var userCount = modixContext.Users.Count();
            var previousUser = modixContext.Users.First(x => x.Id == data.UserId);

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Discriminator.ShouldBe(previousUser.Discriminator);

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
                .Where(x => x.Id != user.Id)
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(NewGuildUserCreationTestCases))]
        public async Task CreateAsync_GuildUserDoesNotExist_InsertsGuildUser(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await uut.CreateAsync(data);

            modixContext.GuildUsers.ShouldContain(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));
            var guildUser = modixContext.GuildUsers.First(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));

            guildUser.Nickname.ShouldBe(data.Nickname);
            guildUser.FirstSeen.ShouldBe(data.FirstSeen);
            guildUser.LastSeen.ShouldBe(data.LastSeen);

            modixContext.GuildUsers.AsEnumerable()
                .Where(x => (x.GuildId != guildUser.GuildId) || (x.UserId != guildUser.UserId))
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .Where(x => (x.UserId != guildUser.UserId) || (x.GuildId != guildUser.GuildId))
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildUserCreationTestCases))]
        public async Task CreateAsync_GuildUserExists_DoesNotUpdateUsersAndGuildUsersAndThrowsException(GuildUserCreationData data)
        {
            (var modixContext, var uut) = BuildTestContext();

            await Should.ThrowAsync<InvalidOperationException>(uut.CreateAsync(data));

            modixContext.GuildUsers.AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
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
                await uut.TryUpdateAsync(1, 1, null!));

            modixContext.GuildUsers.AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
                .EachShould(x => x.ShouldNotHaveChanged());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(ExistingGuildUserIds))]
        public async Task TryUpdateAsync_GuildUserExists_UpdatesGuildUsersAndReturnsTrue(ulong userId, ulong guildId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var user = modixContext.Users.Single(x => x.Id == userId);
            var guildUser = modixContext.GuildUsers.Single(x => (x.UserId == userId) && (x.GuildId == guildId));

            var mutatedData = new GuildUserMutationData()
            {
                Username = "UpdatedUsername",
                Discriminator = "UpdatedDiscriminator",
                Nickname = "UpdatedNickname",
                LastSeen = DateTimeOffset.Now
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
            });

            result.ShouldBeTrue();

            user.Username.ShouldBe(mutatedData.Username);
            user.Discriminator.ShouldBe(mutatedData.Discriminator);
            guildUser.Nickname.ShouldBe(mutatedData.Nickname);
            guildUser.LastSeen.ShouldBe(mutatedData.LastSeen);

            modixContext.GuildUsers.AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .Where(x => (x.UserId != userId) || (x.GuildId != guildId))
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
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

            var result = await uut.TryUpdateAsync(userId, guildId, updateAction);

            result.ShouldBeFalse();

            updateAction.ShouldNotHaveReceived()
                .Invoke(Arg.Any<GuildUserMutationData>());

            modixContext.GuildUsers.AsEnumerable()
                .Select(x => (x.UserId, x.GuildId))
                .ShouldBe(GuildUsers.Entities
                    .Select(x => (x.UserId, x.GuildId)));

            modixContext.GuildUsers
                .EachShould(x => x.ShouldNotHaveChanged());

            modixContext.Users
                .AsQueryable()
                .Select(x => x.Id)
                .ShouldBe(Users.Entities
                    .Select(x => x.Id));

            modixContext.Users
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
