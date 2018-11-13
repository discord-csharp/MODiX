using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;

using Newtonsoft.Json;

using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class GuildUserRepositoryTests
    {
        #region Test Data

        private static readonly string UsersTestDataFilename = @"TestData\Users.json";

        private static readonly string GuildUsersTestDataFilename = @"TestData\GuildUsers.json";

        private static GuildUserCreationData BuildCreationData(ulong userId, ulong guildId)
            => new GuildUserCreationData()
            {
                UserId = userId,
                GuildId = guildId,
                Username = $"NewUser{userId}",
                Discriminator = userId.ToString().PadLeft(4, '0'),
                Nickname = $"NewNickname{userId}",
                FirstSeen = DateTimeOffset.Now - TimeSpan.FromMinutes(userId),
                LastSeen = DateTimeOffset.Now
            };

        private static async Task<ModixContext> BuildTestDataContextAsync(bool useIsolatedInstance = true)
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(nameof(GuildUserRepositoryTests), useIsolatedInstance);

            if (!modixContext.Users.Any())
            {
                modixContext.Users.AddRange(
                    JsonConvert.DeserializeObject<UserEntity[]>(
                        await File.ReadAllTextAsync(UsersTestDataFilename)));

                modixContext.GuildUsers.AddRange(
                    JsonConvert.DeserializeObject<GuildUserEntity[]>(
                        await File.ReadAllTextAsync(GuildUsersTestDataFilename)));

                modixContext.SaveChanges();
            }

            modixContext.ClearReceivedCalls();

            return modixContext;
        }

        #endregion Test Data

        #region Constructor() Tests

        [Test]
        public async Task Constructor_Always_InvokesBaseConstructor()
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: false);

            var uut = new GuildUserRepository(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
        
        #endregion Constructor() Tests

        #region BeginCreateTransactionAsync() Tests

        [Test]
        public async Task BeginCreateTransactionAsync_CreateTransactionIsInProgress_WaitsForCompletion()
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: false);

            var uut = new GuildUserRepository(modixContext);

            var existingTransaction = await uut.BeginCreateTransactionAsync();

            var result = uut.BeginCreateTransactionAsync();

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        public async Task BeginCreateTransactionAsync_CreateTransactionIsNotInProgress_ReturnsImmediately()
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: false);

            var uut = new GuildUserRepository(modixContext);

            var result = uut.BeginCreateTransactionAsync();

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        public async Task BeginCreateTransactionAsync_Always_TransactionIsForContextDatabase()
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: false);

            var database = Substitute.ForPartsOf<DatabaseFacade>(modixContext);
            modixContext.Database.Returns(database);

            var uut = new GuildUserRepository(modixContext);

            using (var transaction = await uut.BeginCreateTransactionAsync()) { }

            await database.ShouldHaveReceived(1)
                .BeginTransactionAsync();
        }

        #endregion BeginCreateTransactionAsync() Tests

        #region CreateAsync() Tests

        [Test]
        public async Task CreateAsync_DataIsNull_ThrowsException()
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: false);

            var uut = new GuildUserRepository(modixContext);

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await uut.CreateAsync(null));

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCase(4UL, 1UL)]
        public async Task CreateAsync_UserDoesNotExist_InsertsUser(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);

            await uut.CreateAsync(data);

            modixContext.Users.ShouldContain(x => x.Id == data.UserId);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 2UL)]
        [TestCase(2UL, 2UL)]
        [TestCase(3UL, 3UL)]
        public async Task CreateAsync_UserExists_UpdatestUser(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);

            var userCount = modixContext.Users.Count();

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(data.Username);
            user.Discriminator.ShouldBe(data.Discriminator);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 2UL)]
        public async Task CreateAsync_UserExistsAndDataUsernameIsNull_DoesNotUpdateUsername(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);
            data.Username = null;

            var userCount = modixContext.Users.Count();
            var previousUser = modixContext.Users.First(x => x.Id == data.UserId);

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Username.ShouldBe(previousUser.Username);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 2UL)]
        public async Task CreateAsync_UserExistsAndDataDiscriminatorIsNull_DoesNotUpdateDiscriminator(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);
            data.Discriminator = null;

            var userCount = modixContext.Users.Count();
            var previousUser = modixContext.Users.First(x => x.Id == data.UserId);

            await uut.CreateAsync(data);

            modixContext.Users.Count().ShouldBe(userCount);
            var user = modixContext.Users.First(x => x.Id == data.UserId);

            user.Discriminator.ShouldBe(previousUser.Discriminator);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 2UL)]
        [TestCase(2UL, 2UL)]
        [TestCase(3UL, 3UL)]
        [TestCase(4UL, 1UL)]
        public async Task CreateAsync_GuildUserDoesNotExist_InsertsGuildUser(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);

            await uut.CreateAsync(data);

            modixContext.GuildUsers.ShouldContain(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));
            var guildUser = modixContext.GuildUsers.First(x => (x.GuildId == data.GuildId) && (x.UserId == data.UserId));

            guildUser.Nickname.ShouldBe(data.Nickname);
            guildUser.FirstSeen.ShouldBe(data.FirstSeen);
            guildUser.LastSeen.ShouldBe(data.LastSeen);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 1UL)]
        [TestCase(2UL, 1UL)]
        [TestCase(3UL, 2UL)]
        public async Task CreateAsync_GuildUserExists_ThrowsException(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var data = BuildCreationData(userId, guildId);

            await Should.ThrowAsync<InvalidOperationException>(uut.CreateAsync(data));

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion CreateAsync() Tests

        #region ReadSummaryAsync() Tests

        [TestCase(1UL, 1UL)]
        [TestCase(2UL, 1UL)]
        [TestCase(3UL, 2UL)]
        public async Task ReadSummaryAsync_GuildUserExists_ReturnsGuildUser(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync();

            var uut = new GuildUserRepository(modixContext);

            var result = await uut.ReadSummaryAsync(userId, guildId);

            var user = modixContext.Users.Single(x => x.Id == userId);
            var guildUser = modixContext.GuildUsers.Single(x => (x.UserId == userId) && (x.GuildId == guildId));

            result.UserId.ShouldBe(userId);
            result.GuildId.ShouldBe(guildId);
            result.Username.ShouldBe(user.Username);
            result.Discriminator.ShouldBe(user.Discriminator);
            result.Nickname.ShouldBe(guildUser.Nickname);
            result.FirstSeen.ShouldBe(guildUser.FirstSeen);
            result.LastSeen.ShouldBe(guildUser.LastSeen);
        }

        [TestCase(1UL, 2UL)]
        [TestCase(2UL, 2UL)]
        [TestCase(3UL, 3UL)]
        [TestCase(4UL, 1UL)]
        public async Task ReadSummaryAsync_GuildUserDoesNotExist_ReturnsNull(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync();

            var uut = new GuildUserRepository(modixContext);

            var result = await uut.ReadSummaryAsync(userId, guildId);

            result.ShouldBeNull();
        }

        #endregion ReadSummaryAsync() Tests

        #region TryUpdateAsync() Tests

        [Test]
        public async Task TryUpdateAsync_UpdateActionIsNull_ThrowsException()
        {
            var modixContext = await BuildTestDataContextAsync();

            var uut = new GuildUserRepository(modixContext);

            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await uut.TryUpdateAsync(1, 1, null));

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        [TestCase(1UL, 1UL)]
        [TestCase(2UL, 1UL)]
        [TestCase(3UL, 2UL)]
        public async Task TryUpdateAsync_GuildUserExists_UpdatesGuildUserAndReturnsTrue(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

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

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCase(1UL, 2UL)]
        [TestCase(2UL, 2UL)]
        [TestCase(3UL, 3UL)]
        [TestCase(4UL, 1UL)]
        public async Task TryUpdateAsync_GuildUserDoesNotExist_DoesNotPerformUpdateAndReturnsFalse(ulong userId, ulong guildId)
        {
            var modixContext = await BuildTestDataContextAsync(useIsolatedInstance: true);

            var uut = new GuildUserRepository(modixContext);

            var updateAction = Substitute.For<Action<GuildUserMutationData>>();

            var result = await uut.TryUpdateAsync(userId, guildId, updateAction);

            result.ShouldBeFalse();

            updateAction.ShouldNotHaveReceived()
                .Invoke(Arg.Any<GuildUserMutationData>());

            await modixContext.ShouldNotHaveReceived()
                .SaveChangesAsync();
        }

        #endregion TryUpdateAsync() Tests
    }
}
