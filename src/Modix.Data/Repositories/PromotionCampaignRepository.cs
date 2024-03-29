using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="PromotionCampaignEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IPromotionCampaignRepository
    {
        /// <summary>
        /// Begins a new transaction to create campaigns within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Begins a new transaction to close campaigns within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCloseTransactionAsync();

        /// <summary>
        /// Creates a new campaign within the repository.
        /// </summary>
        /// <param name="data">The data for the campaign to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the action representing the creation of the campaign.
        /// </returns>
        Task<PromotionActionSummary> CreateAsync(PromotionCampaignCreationData data);

        /// <summary>
        /// Checks whether the repository contains any campaigns matching the given search criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="PromotionCampaignEntity.Id"/> values to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching campaigns exist.
        /// </returns>
        Task<bool> AnyAsync(PromotionCampaignSearchCriteria searchCriteria);

        /// <summary>
        /// Searches the repository for campaign information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="PromotionCampaignSummary"/> records to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the campaign records that match <paramref name="searchCriteria"/>.</returns>
        Task<IReadOnlyCollection<PromotionCampaignSummary>> SearchSummariesAsync(PromotionCampaignSearchCriteria searchCriteria);

        /// <summary>
        /// Retrieves detailed information about a campaign, based on its ID.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value of the campaign to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested campaign, or null if no such campaign exists.
        /// </returns>
        Task<PromotionCampaignDetails?> ReadDetailsAsync(long campaignId);

        /// <summary>
        /// Marks an existing campaign as closed, based on its ID.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value of the campaign to be deleted.</param>
        /// <param name="deletedById">The Discord snowflake ID value of the user that is deleting the infraction.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the action representing the closure of the campaign.
        /// </returns>
        Task<PromotionActionSummary?> TryCloseAsync(long campaignId, ulong closedById, PromotionCampaignOutcome outcome);

        /// <summary>
        /// Retrieves the promotion progression for the supplied user.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild in which the desired promotions took place.</param>
        /// <param name="userId">The unique Discord snowflake ID of the user for whom to retrieve promotions.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection representing the promotion progression for the supplied user.
        /// </returns>
        Task<IReadOnlyCollection<PromotionCampaignSummary>> GetPromotionsForUserAsync(ulong guildId, ulong userId);
    }

    /// <inheritdoc />
    public class PromotionCampaignRepository : RepositoryBase, IPromotionCampaignRepository
    {
        /// <summary>
        /// Creates a new <see cref="PromotionCampaignRepository"/>, with the injected dependencies
        /// </summary>
        public PromotionCampaignRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCloseTransactionAsync()
            => _closeTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<PromotionActionSummary> CreateAsync(PromotionCampaignCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<PromotionCampaignEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.CampaignId = entity.Id;
            await ModixContext.SaveChangesAsync();

            var action = await ModixContext.Set<PromotionActionEntity>().AsNoTracking()
                .Where(x => x.Id == entity.CreateActionId)
                .AsExpandable()
                .Select(PromotionActionSummary.FromEntityProjection)
                .FirstAsync();

            return action;
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(PromotionCampaignSearchCriteria searchCriteria)
            => ModixContext.Set<PromotionCampaignEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AnyAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PromotionCampaignSummary>> SearchSummariesAsync(PromotionCampaignSearchCriteria searchCriteria)
            => await ModixContext.Set<PromotionCampaignEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(PromotionCampaignSummary.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public Task<PromotionCampaignDetails?> ReadDetailsAsync(long campaignId)
            => ModixContext.Set<PromotionCampaignEntity>().AsNoTracking()
                .Where(x => x.Id == campaignId)
                .AsExpandable()
                .Select(PromotionCampaignDetails.FromEntityProjection)
                .FirstOrDefaultAsync<PromotionCampaignDetails?>();

        /// <inheritdoc />
        public async Task<PromotionActionSummary?> TryCloseAsync(long campaignId, ulong closedById, PromotionCampaignOutcome outcome)
        {
            var entity = await ModixContext.Set<PromotionCampaignEntity>()
                .Where(x => x.Id == campaignId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.CloseActionId != null))
                return null;

            entity.Outcome = outcome;
            entity.CloseAction = new PromotionActionEntity()
            {
                GuildId = entity.GuildId,
                Type = PromotionActionType.CampaignClosed,
                Created = DateTimeOffset.UtcNow,
                CreatedById = closedById,
                CampaignId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            var action = await ModixContext.Set<PromotionActionEntity>().AsNoTracking()
                .Where(x => x.Id == entity.CloseActionId)
                .AsExpandable()
                .Select(PromotionActionSummary.FromEntityProjection)
                .FirstAsync();

            return action;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PromotionCampaignSummary>> GetPromotionsForUserAsync(ulong guildId, ulong userId)
            => await ModixContext.Set<PromotionCampaignEntity>().AsNoTracking()
                .Where(x => x.GuildId == guildId
                    && x.SubjectId == userId
                    && x.Outcome == PromotionCampaignOutcome.Accepted)
                .AsExpandable()
                .Select(PromotionCampaignSummary.FromEntityProjection)
                .ToArrayAsync();

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _closeTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
