using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using Modix.Data.Repositories;

using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class RepositoryTransactionFactoryTests
    {
        #region BeginTransactionAsync(database) Tests

        [Test]
        public async Task BeginTransactionAsync_RepositoryTransactionIsInProgress_WaitsForCompletion()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            var existingTransaction = await uut.BeginTransactionAsync(database);

            var result = uut.BeginTransactionAsync(database);

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        public async Task BeginTransactionAsync_RepositoryTransactionIsNotInProgress_ReturnsImmediately()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            var result = uut.BeginTransactionAsync(database);

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        public async Task BeginTransactionAsync_DatabaseDoesNotHaveCurrentTransaction_BeginsDatabaseTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(null as IDbContextTransaction);

            using (var transaction = await uut.BeginTransactionAsync(database))
            {
                await database.ShouldHaveReceived(1)
                    .BeginTransactionAsync();
            }
        }

        [Test]
        public async Task BeginTransactionAsync_DatabaseHasCurrentTransaction_DoesNotBeginDatabaseTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(Substitute.For<IDbContextTransaction>());

            using (var transaction = await uut.BeginTransactionAsync(database))
            {
                await database.ShouldNotHaveReceived()
                    .BeginTransactionAsync();
            }
        }

        #endregion BeginTransactionAsync(database) Tests

        #region BeginTransactionAsync(database, cancellationToken) Tests

        [Test]
        public async Task BeginTransactionAsyncWithToken_RepositoryTransactionIsInProgress_WaitsForExistingTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            var existingTransaction = await uut.BeginTransactionAsync(database);

            var result = uut.BeginTransactionAsync(database, cancellationTokenSource.Token);

            result.IsCompleted.ShouldBeFalse();

            existingTransaction.Dispose();
            (await result).Dispose();
        }

        [Test]
        public async Task BeginTransactionAsyncWithToken_RepositoryTransactionIsNotInProgress_ReturnsRepositoryTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            var result = uut.BeginTransactionAsync(database, cancellationTokenSource.Token);

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        public async Task BeginTransactionAsyncWithToken_DatabaseDoesNotHaveCurrentTransaction_BeginsDatabaseTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            database.CurrentTransaction.Returns(null as IDbContextTransaction);

            using (var transaction = await uut.BeginTransactionAsync(database, cancellationTokenSource.Token))
            {
                await database.ShouldHaveReceived(1)
                    .BeginTransactionAsync(cancellationTokenSource.Token);
            }
        }

        [Test]
        public async Task BeginTransactionAsyncWithToken_DatabaseHasCurrentTransaction_DoesNotBeginDatabaseTransaction()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            database.CurrentTransaction.Returns(Substitute.For<IDbContextTransaction>());

            using (var transaction = await uut.BeginTransactionAsync(database, cancellationTokenSource.Token))
            {
                await database.ShouldNotHaveReceived()
                    .BeginTransactionAsync(cancellationTokenSource.Token);
            }
        }

        [Test]
        public async Task BeginTransactionAsyncWithToken_CancellationTokenIsSet_CancelsTask()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            using (var existingTransaction = await uut.BeginTransactionAsync(database))
            {
                var result = uut.BeginTransactionAsync(database, cancellationTokenSource.Token);

                cancellationTokenSource.Cancel();

                Should.Throw<TaskCanceledException>(result);
            }
        }

        [Test]
        public async Task BeginTransactionAsyncWithToken_CancellationTokenIsSet_FactoryIsNotLocked()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());
            var cancellationTokenSource = new CancellationTokenSource();

            using (var existingTransaction = await uut.BeginTransactionAsync(database))
            {
                #pragma warning disable CS4014
                uut.BeginTransactionAsync(database, cancellationTokenSource.Token);
                #pragma warning restore CS4014

                cancellationTokenSource.Cancel();
            }

            var result = uut.BeginTransactionAsync(database);

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        #endregion BeginTransactionAsync(database, cancellationToken) Tests

        #region RepositoryTransaction Tests

        [Test]
        public async Task RepositoryTransaction_DatabaseDoesNotHaveCurrentTransaction_CommitsDatabaseTransactionOnCommit()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(null as IDbContextTransaction);

            var databaseTransaction = Substitute.For<IDbContextTransaction>();
            database.BeginTransactionAsync().Returns(databaseTransaction);

            using (var transaction = await uut.BeginTransactionAsync(database))
            {
                transaction.Commit();

                databaseTransaction.ShouldHaveReceived(1)
                    .Commit();
            }
        }

        [Test]
        public async Task RepositoryTransaction_DatabaseHasCurrentTransaction_DoesNotThrowExceptionOnCommit()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(Substitute.For<IDbContextTransaction>());

            using (var transaction = await uut.BeginTransactionAsync(database))
            {
                Should.NotThrow(() =>
                {
                    transaction.Commit();
                });
            }
        }

        [Test]
        public async Task RepositoryTransaction_DatabaseDoesNotHaveCurrentTransaction_RollsBackDatabaseTransactionOnDispose()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(null as IDbContextTransaction);

            var databaseTransaction = Substitute.For<IDbContextTransaction>();
            database.BeginTransactionAsync().Returns(databaseTransaction);

            var transaction = await uut.BeginTransactionAsync(database);

            transaction.Dispose();

            databaseTransaction.ShouldHaveReceived(1)
                .Rollback();
        }

        [Test]
        public async Task RepositoryTransaction_DatabaseHasCurrentTransaction_DoesNotThrowExceptionOnDispose()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            database.CurrentTransaction.Returns(Substitute.For<IDbContextTransaction>());

            var transaction = await uut.BeginTransactionAsync(database);

            Should.NotThrow(() =>
            {
                transaction.Dispose();
            });
        }

        [Test]
        public async Task RepositoryTransaction_HasCommitted_DoesNotCommitDatabaseTransactionOnCommit()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            var databaseTransaction = Substitute.For<IDbContextTransaction>();
            database.BeginTransactionAsync().Returns(databaseTransaction);

            using (var transaction = await uut.BeginTransactionAsync(database))
            {
                transaction.Commit();
                databaseTransaction.ClearReceivedCalls();

                transaction.Commit();

                databaseTransaction.ShouldNotHaveReceived()
                    .Commit();
            }
        }

        [Test]
        public async Task RepositoryTransaction_HasCommitted_DoesNotRollbackDatabaseTransactionOnDispose()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            var databaseTransaction = Substitute.For<IDbContextTransaction>();
            database.BeginTransactionAsync().Returns(databaseTransaction);

            var transaction = await uut.BeginTransactionAsync(database);

            transaction.Commit();
            databaseTransaction.ClearReceivedCalls();

            transaction.Dispose();

            databaseTransaction.ShouldNotHaveReceived()
                .Rollback();
        }

        [Test]
        public async Task RepositoryTransaction_HasCommitted_FactoryIsNotLocked()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            using (var existingTransaction = await uut.BeginTransactionAsync(database))
            {
                existingTransaction.Commit();
            }

            var result = uut.BeginTransactionAsync(database);

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        [Test]
        public async Task RepositoryTransaction_HasDisposed_FactoryIsNotLocked()
        {
            var uut = new RepositoryTransactionFactory();
            var database = Substitute.For<DatabaseFacade>(Substitute.For<DbContext>());

            using (var existingTransaction = await uut.BeginTransactionAsync(database))
            {
                existingTransaction.Dispose();
            }

            var result = uut.BeginTransactionAsync(database);

            result.IsCompleted.ShouldBeTrue();

            (await result).Dispose();
        }

        #endregion RepositoryTransaction Tests
    }
}
