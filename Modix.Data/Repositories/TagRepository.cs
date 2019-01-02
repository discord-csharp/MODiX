using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Tags;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing tag entities within an underlying data storage provider.
    /// </summary>
    public interface ITagRepository
    {
        /// <summary>
        /// Begins a new transaction to maintain tags within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginMaintainTransactionAsync();

        /// <summary>
        /// Begins a new transaction to use tags within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginUseTransactionAsync();

        /// <summary>
        /// Creates a new tag within the repository.
        /// </summary>
        /// <param name="data">The data for the tag to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated identifier value assigned to the new tag.
        /// </returns>
        Task<long> CreateAsync(TagCreationData data);

        /// <summary>
        /// Retrieves information about a tag based on its guild and name.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name of the tag within the guild.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested tag, or null if no such tag exists.
        /// </returns>
        Task<TagSummary> ReadSummaryAsync(ulong guildId, string name);

        /// <summary>
        /// Retrieves a collection of tags based on the supplied search criteria.
        /// </summary>
        /// <param name="searchCriteria">Criteria describing how to filter the result set of tags.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="searchCriteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of tags that have similar names to the supplied name.
        /// </returns>
        Task<IReadOnlyCollection<TagSummary>> SearchSummariesAsync(TagSearchCriteria searchCriteria);

        /// <summary>
        /// Increments the usage counter on the supplied tag.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name of the tag within the guild.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation succeeded.
        /// </returns>
        Task<bool> TryIncrementUsesAsync(ulong guildId, string name);

        /// <summary>
        /// Modifies an existing tag within the repository.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name of the tag within the guild.</param>
        /// <param name="modifiedByUserId">The Discord snowflake ID value of the user who modified the tag.</param>
        /// <param name="modifyAction">A delegate that describes how to modify the tag.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="modifyAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation succeeded.
        /// </returns>
        Task<bool> TryModifyAsync(ulong guildId, string name, ulong modifiedByUserId, Action<TagMutationData> modifyAction);

        /// <summary>
        /// Deletes an existing tag within the repository.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID value of the guild to which the tag belongs.</param>
        /// <param name="name">The name of the tag within the guild.</param>
        /// <param name="deletedByUserId">The Discord snowflake ID value of the user who deleted the tag.</param>
        /// <exception cref="ArgumentException">Throws for <paramref name="name"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation succeeded.
        /// </returns>
        Task<bool> TryDeleteAsync(ulong guildId, string name, ulong deletedByUserId);
    }

    /// <inheritdoc />
    public class TagRepository : RepositoryBase, ITagRepository
    {
        /// <summary>
        /// Creates a new <see cref="TagRepository"/> with the injected dependencies
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public TagRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginMaintainTransactionAsync()
            => _maintainTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginUseTransactionAsync()
            => _useTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(TagCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Tags.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.NewTagId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task<TagSummary> ReadSummaryAsync(ulong guildId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The supplied name cannot be null or whitespace.", nameof(name));

            return await ModixContext.Tags.AsNoTracking()
                .Where(x
                    => x.GuildId == guildId
                    && x.Name == name.ToLower()
                    && x.DeleteActionId == null)
                .AsExpandable()
                .Select(TagSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<TagSummary>> SearchSummariesAsync(TagSearchCriteria searchCriteria)
        {
            if (searchCriteria is null)
                throw new ArgumentNullException(nameof(searchCriteria));

            return await ModixContext.Tags.AsNoTracking()
                .Where(x => x.DeleteActionId == null)
                .FilterBy(searchCriteria)
                .OrderBy(x => x.Name)
                .Take(20)
                .AsExpandable()
                .Select(TagSummary.FromEntityProjection)
                .ToArrayAsync();
        }

        /// <inheritdoc />
        public async Task<bool> TryIncrementUsesAsync(ulong guildId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The supplied name cannot be null or whitespace.", nameof(name));

            var entity = await ModixContext.Tags.FirstOrDefaultAsync(x
                => x.GuildId == guildId
                && x.Name == name.ToLower()
                && x.DeleteActionId == null);

            if (entity is null)
                return false;

            entity.Uses++;

            await ModixContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryModifyAsync(ulong guildId, string name, ulong modifiedByUserId, Action<TagMutationData> modifyAction)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The supplied name cannot be null or whitespace.", nameof(name));

            if (modifyAction is null)
                throw new ArgumentNullException(nameof(modifyAction));

            var oldTag = await ModixContext.Tags.FirstOrDefaultAsync(x
                => x.GuildId == guildId
                && x.Name == name.ToLower()
                && x.DeleteActionId == null);

            if (oldTag is null)
                return false;

            var action = new TagActionEntity()
            {
                GuildId = oldTag.GuildId,
                Type = TagActionType.TagModified,
                Created = DateTimeOffset.Now,
                CreatedById = modifiedByUserId,
                OldTagId = oldTag.Id,
            };

            await ModixContext.TagActions.AddAsync(action);
            await ModixContext.SaveChangesAsync();

            var newTagData = TagMutationData.FromEntity(oldTag);
            modifyAction(newTagData);

            var newTag = newTagData.ToEntity();
            newTag.CreateActionId = action.Id;

            await ModixContext.Tags.AddAsync(newTag);
            await ModixContext.SaveChangesAsync();

            action.NewTagId = newTag.Id;
            oldTag.DeleteActionId = action.Id;

            await ModixContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(ulong guildId, string name, ulong deletedByUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The supplied name cannot be null or whitespace.", nameof(name));

            var entity = await ModixContext.Tags.FirstOrDefaultAsync(x
                => x.GuildId == guildId
                && x.Name == name.ToLower()
                && x.DeleteActionId == null);

            if (entity is null)
                return false;

            var deleteAction = new TagActionEntity()
            {
                GuildId = entity.GuildId,
                Created = DateTimeOffset.Now,
                Type = TagActionType.TagDeleted,
                CreatedById = deletedByUserId,
                OldTagId = entity.Id,
            };

            await ModixContext.TagActions.AddAsync(deleteAction);
            await ModixContext.SaveChangesAsync();

            entity.DeleteActionId = deleteAction.Id;
            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _maintainTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _useTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
