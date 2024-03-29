using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="GuildRoleEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IGuildRoleRepository
    {
        /// <summary>
        /// Begins a new transaction to create roles within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new set of role data within the repository.
        /// </summary>
        /// <param name="data">The initial set of role data to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task CreateAsync(GuildRoleCreationData data);

        /// <summary>
        /// Attempts to update information about a role, based on its ID value.
        /// </summary>
        /// <param name="roleId">The <see cref="GuildRoleEntity.RoleId"/> value of the user guild data to be updated.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether the requested update succeeded (I.E. whether the specified data record exists).
        /// </returns>
        Task<bool> TryUpdateAsync(ulong roleId, Action<GuildRoleMutationData> updateAction);
    }

    /// <inheritdoc />
    public class GuildRoleRepository : RepositoryBase, IGuildRoleRepository
    {
        /// <summary>
        /// Creates a new <see cref="GuildRoleRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public GuildRoleRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(GuildRoleCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<GuildRoleEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> TryUpdateAsync(ulong roleId, Action<GuildRoleMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.Set<GuildRoleEntity>()
                .Where(x => x.RoleId == roleId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = GuildRoleMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.Name);
            ModixContext.UpdateProperty(entity, x => x.Position);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
