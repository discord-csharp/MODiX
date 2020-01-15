using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using Nito.AsyncEx;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Creates <see cref="IRepositoryTransaction"/> objects for a repository to publish to its consumers.
    /// </summary>
    public class RepositoryTransactionFactory
    {
        /// <summary>
        /// Returns a new <see cref="IRepositoryTransaction"/> object, which creates a new transaction upon the given database,
        /// and manages its lifetime.
        /// 
        /// Only one transaction created by this provider may exist at any given time. If this method is called while a transaction
        /// already exists, it will not (asynchronously) return until that transaction is complete.
        /// </summary>
        /// <param name="database">The database upon which a transaction is to be performed.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="database"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the requested transaction object can be created,
        /// containing the requested transaction object.
        /// </returns>
        public async Task<IRepositoryTransaction> BeginTransactionAsync(DatabaseFacade database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return new RepositoryTransaction(
                (database.CurrentTransaction is null)
                    ? await database.BeginTransactionAsync()
                    : null,
                await _lockProvider.LockAsync());
        }

        /// <summary>
        /// Returns a new <see cref="IRepositoryTransaction"/> object, which creates a new transaction upon the given database,
        /// and manages its lifetime.
        /// 
        /// Only one transaction created by this provider may exist at any given time. If this method is called while a transaction
        /// already exists, it will not (asynchronously) return until that transaction is complete.
        /// </summary>
        /// <param name="database">The database upon which a transaction is to be performed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the operation early.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="database"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the requested transaction object can be created,
        /// containing the requested transaction object.
        /// </returns>
        public async Task<IRepositoryTransaction> BeginTransactionAsync(DatabaseFacade database, CancellationToken cancellationToken)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return new RepositoryTransaction(
                (database.CurrentTransaction is null)
                    ? await database.BeginTransactionAsync(cancellationToken)
                    : null,
                await _lockProvider.LockAsync(cancellationToken));
        }

        private AsyncLock _lockProvider { get; }
            = new AsyncLock();

        private class RepositoryTransaction : IRepositoryTransaction
        {
            public RepositoryTransaction(IDbContextTransaction? transaction, IDisposable @lock)
            {
                _transaction = transaction;
                _lock = @lock;
            }

            public void Commit()
            {
                if (!_hasCommitted)
                {
                    _transaction?.Commit();
                    _hasCommitted = true;
                }
            }

            public void Dispose()
            {
                if(!_hasDisposed)
                {
                    if (!_hasCommitted)
                        _transaction?.Rollback();

                    _lock.Dispose();

                    _hasDisposed = true;
                }
            }

            private bool _hasCommitted
                = false;

            private bool _hasDisposed
                = false;

            private readonly IDbContextTransaction? _transaction;

            private readonly IDisposable _lock;
        }
    }
}
