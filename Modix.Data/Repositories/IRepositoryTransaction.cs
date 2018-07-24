using System;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a object used to synchronize multi-step create or update operations to be performed upon a repository.
    /// 
    /// A repository transaction is a mechanism for reliably rolling back multi-step operations, if a single step fails,
    /// and for ensuring concurrency between operations being run in parallel.
    /// </summary>
    public interface IRepositoryTransaction : IDisposable
    {
        /// <summary>
        /// Commits any changes performed during the transaction to be written to the underlying data storage provider.
        /// 
        /// This should usually be called right before <see cref="IDisposable.Dispose"/>, and may only be called once per transaction.
        /// 
        /// If this method is not called before <see cref="IDisposable.Dispose"/>, all changes are automatically rolled back.
        /// </summary>
        void Commit();
    }
}
