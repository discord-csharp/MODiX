using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="InfractionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IInfractionRepository
    {
        /// <summary>
        /// Inserts a new <see cref="InfractionEntity"/> into the repository.
        /// </summary>
        /// <param name="infraction">
        /// The <see cref="InfractionEntity"/> to be inserted.
        /// The <see cref="InfractionEntity.Id"/> values are generated automatically.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to <paramref name="infraction"/>.
        /// </returns>
        Task<long> InsertAsync(InfractionEntity infraction);

        /// <summary>
        /// Updates the <see cref="InfractionEntity.Duration"/> value of an existing <see cref="InfractionEntity"/> within the repository.
        /// </summary>
        /// <param name="id">The <see cref="InfractionEntity.Id"/> value of the <see cref="InfractionEntity"/> to be updated.</param>
        /// <param name="duration">The new <see cref="InfractionEntity.Duration"/> value to be saved into the repository.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateDurationAsync(long id, TimeSpan duration);

        /// <summary>
        /// Updates the <see cref="InfractionEntity.IsRescinded"/> value of an existing <see cref="InfractionEntity"/> within the repository.
        /// </summary>
        /// <param name="id">The <see cref="InfractionEntity.Id"/> value of the <see cref="InfractionEntity"/> to be updated.</param>
        /// <param name="isRescinded">The new <see cref="InfractionEntity.IsRescinded"/> value to be saved into the repository.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateIsRescindedAsync(long id, bool isRescinded);

        /// <summary>
        /// Searches the repository for <see cref="InfractionEntity"/> entities, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionEntity"/> entities to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching entities to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested entities have been retrieved.</returns>
        Task<QueryPage<InfractionEntity>> SearchAsync(InfractionSearchCriteria searchCriteria, PagingCriteria pagingCriteria);
    }

    /// <inheritdoc />
    public class InfractionRepository : IInfractionRepository
    {
        public InfractionRepository(ModixContext modixContext)
        {
            ModixContext = modixContext ?? throw new ArgumentNullException(nameof(modixContext));
        }

        /// <inheritdoc />
        public async Task<long> InsertAsync(InfractionEntity infraction)
        {
            await ModixContext.Infractions.AddAsync(infraction);

            await ModixContext.SaveChangesAsync();

            return infraction.Id;
        }

        public async Task<QueryPage<InfractionEntity>> SearchAsync(InfractionSearchCriteria searchCriteria, PagingCriteria pagingCriteria)
            => await new QueryPageBuilder<InfractionEntity>()
            {
                Query = ModixContext.Infractions,
                WhereClause = query =>
                {
                    if ((searchCriteria.Types != null) && (searchCriteria.Types.Count > 0))
                        query = query.Where(x => searchCriteria.Types.Contains(x.Type));

                    if (searchCriteria.SubjectId.HasValue)
                        query = query.Where(x => x.SubjectId == searchCriteria.SubjectId.Value);

                    if ((searchCriteria.CreatedRange.HasValue) && (searchCriteria.CreatedRange.Value.From.HasValue))
                        query = query.Where(x => ModixContext.ModerationActions.Any(y =>
                            (y.InfractionId == x.Id) && (y.Type == ModerationActionTypes.InfractionCreated)
                            && (y.Created >= searchCriteria.CreatedRange.Value.From.Value)));

                    if ((searchCriteria.CreatedRange.HasValue) && (searchCriteria.CreatedRange.Value.To.HasValue))
                        query = query.Where(x => ModixContext.ModerationActions.Any(y =>
                            (y.InfractionId == x.Id) && (y.Type == ModerationActionTypes.InfractionCreated)
                            && (y.Created <= searchCriteria.CreatedRange.Value.To.Value)));

                    if (searchCriteria.CreatedById.HasValue)
                        query = query.Where(x => ModixContext.ModerationActions.Any(y =>
                            (y.InfractionId == x.Id) && (y.Type == ModerationActionTypes.InfractionCreated)
                            && (y.CreatedById == searchCriteria.CreatedById.Value)));

                    if (searchCriteria.IsExpired.HasValue)
                        query = query.Where(x => x.Duration.HasValue
                            && ((ModixContext.ModerationActions.First(y => (y.InfractionId == x.Id) && (y.Type == ModerationActionTypes.InfractionCreated))
                                    .Created + x.Duration.Value)
                                > DateTimeOffset.UtcNow)
                            == searchCriteria.IsExpired.Value);

                    if (searchCriteria.IsRescinded.HasValue)
                        query = query.Where(x => x.IsRescinded == searchCriteria.IsRescinded.Value);

                    return query;
                },
            }.BuildAsync(pagingCriteria);

        public async Task UpdateDurationAsync(long id, TimeSpan duration)
        {
            var infraction = new InfractionEntity()
            {
                Id = id,
                Duration = duration
            };

            await ModixContext.UpdateEntityPropertiesAsync(infraction, x => x.Duration);
        }

        public async Task UpdateIsRescindedAsync(long id, bool isRescinded)
        {
            var infraction = new InfractionEntity()
            {
                Id = id,
                IsRescinded = isRescinded
            };

            await ModixContext.UpdateEntityPropertiesAsync(infraction, x => x.IsRescinded);
        }

        internal protected ModixContext ModixContext { get; }
    }
}
