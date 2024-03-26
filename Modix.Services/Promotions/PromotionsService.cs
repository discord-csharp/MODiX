#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Humanizer;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Promotions
{
    /// <summary>
    /// Describes a service for interacting with the promotions system.
    /// </summary>
    public interface IPromotionsService
    {
        /// <summary>
        /// Creates a new promotion campaign, and attaches an initial comment to it.
        /// </summary>
        /// <param name="subjectId">The Discord snowflake ID of the user whose promotion is being proposed.</param>
        /// <param name="comment">The content of the comment to be added to the new campaign.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task CreateCampaignAsync(ulong subjectId, string? comment = null);

        /// <summary>
        /// Searches for the next rank role available to the supplied user.
        /// </summary>
        /// <param name="subjectId">The Discord snowflake ID of the user for whom the next rank is to be determined.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the operation has completed,
        /// containing the next rank role for the user or null if no such rank could be determined.
        /// </returns>
        Task<GuildRoleBrief?> GetNextRankRoleForUserAsync(ulong subjectId);

        /// <summary>
        /// Adds a comment to an active promotion campaign.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value of the campaign to which the new comment is to be added.</param>
        /// <param name="sentiment">The <see cref="PromotionCommentEntity.Sentiment"/> value to use for the new comment.</param>
        /// <param name="content">The <see cref="PromotionCommentEntity.Content"/> value to use for the new comment.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task<PromotionActionSummary> AddCommentAsync(long campaignId, PromotionSentiment sentiment, string? content);

        /// <summary>
        /// Updates an existing comment on a promotion campaign by deleting the comment and adding a new one.
        /// </summary>
        /// <param name="commentId">The <see cref="PromotionCommentEntity.Id"/> value of the comment that is being updated.</param>
        /// <param name="newSentiment">The <see cref="PromotionCommentEntity.Sentiment"/> value of the updated comment.</param>
        /// <param name="newContent">The <see cref="PromotionCommentEntity.Content"/> value of the updated comment.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task<PromotionActionSummary> UpdateCommentAsync(long commentId, PromotionSentiment newSentiment, string? newContent);

        Task AddOrUpdateCommentAsync(long campaignId, Optional<PromotionSentiment> sentiment, Optional<string?> comment = default);

        /// <summary>
        /// Closes a campaign, with an <see cref="PromotionCampaignEntity.Outcome"/> value of <see cref="PromotionCampaignOutcome.Accepted"/>,
        /// and executes the proposed promotion.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value fo the campaign to be accepted.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AcceptCampaignAsync(long campaignId, bool force);

        /// <summary>
        /// Closes a campaign, with an <see cref="PromotionCampaignEntity.Outcome"/> value of <see cref="PromotionCampaignOutcome.Rejected"/>,
        /// without executing the proposed promotion.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value fo the campaign to be rejected.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RejectCampaignAsync(long campaignId);

        /// <summary>
        /// Retrieves a collection of promotion campaign summaries, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which campaigns are to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of campaigns.
        /// </returns>
        Task<IReadOnlyCollection<PromotionCampaignSummary>> SearchCampaignsAsync(PromotionCampaignSearchCriteria searchCriteria);

        /// <summary>
        /// Retrieves detailed information about a particular promotion campaign, based on its ID value.
        /// </summary>
        /// <param name="campaignId">The <see cref="PromotionCampaignEntity.Id"/> value of the promotion campaign to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested campaign information.
        /// </returns>
        Task<PromotionCampaignDetails?> GetCampaignDetailsAsync(long campaignId);

        /// <summary>
        /// Retrieves information about a particular action that occurred within the promotions system, based on its ID value.
        /// </summary>
        /// <param name="promotionActionId">The <see cref="PromotionActionEntity.Id"/> value of the action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested action information.
        /// </returns>
        Task<PromotionActionSummary?> GetPromotionActionSummaryAsync(long promotionActionId);

        /// <summary>
        /// Retrieves the promotion progression for the supplied user.
        /// </summary>
        /// <param name="guildId">The unique Discord snowflake ID of the guild in which the desired promotions took place.</param>
        /// <param name="userId">The unique Discord snowflake ID of the user for whom to retrieve promotions.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation is complete,
        /// containing a collection representing the promotion progression for the supplied user.
        /// </returns>
        Task<IReadOnlyCollection<PromotionCampaignSummary>> GetPromotionsForUserAsync(ulong guildId, ulong userId);

        Task<PromotionCommentSummary[]> SearchPromotionCommentsAsync(PromotionCommentSearchCriteria searchCriteria);
    }

    /// <inheritdoc />
    public class PromotionsService : IPromotionsService
    {
        public PromotionsService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IUserService userService,
            IDesignatedRoleService designatedRoleService,
            IPromotionActionRepository promotionActionRepository,
            IPromotionCampaignRepository promotionCampaignRepository,
            IPromotionCommentRepository promotionCommentRepository,
            IMessageDispatcher messageDispatcher)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            UserService = userService;
            DesignatedRoleService = designatedRoleService;
            PromotionActionRepository = promotionActionRepository;
            PromotionCampaignRepository = promotionCampaignRepository;
            PromotionCommentRepository = promotionCommentRepository;
            MessageDispatcher = messageDispatcher;
        }

        /// <inheritdoc />
        public async Task CreateCampaignAsync(ulong subjectId, string? comment = null)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            ValidateCreateCampaignAuthorization();

            var rankRoles = await GetRankRolesAsync(AuthorizationService.CurrentGuildId.Value);
            var subject = await UserService.GetGuildUserAsync(AuthorizationService.CurrentGuildId.Value, subjectId);

            if (subject.IsBot)
            {
                throw new InvalidOperationException("Bots cannot be nominated for a promotion campaign.");
            }

            if (!TryGetNextRankRoleForUser(rankRoles, subject, out var nextRankRole, out var message))
                throw new InvalidOperationException(message);

            await PerformCommonCreateCampaignValidationsAsync(subject, nextRankRole, rankRoles);
            await FinalizeCreateCampaignAsync(subjectId, nextRankRole.Id, comment);
        }

        /// <inheritdoc />
        public async Task<GuildRoleBrief?> GetNextRankRoleForUserAsync(ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();

            if (subjectId == 0)
                return null;

            var rankRoles = await GetRankRolesAsync(AuthorizationService.CurrentGuildId.Value);
            var subject = await UserService.GetGuildUserAsync(AuthorizationService.CurrentGuildId.Value, subjectId);

            if (TryGetNextRankRoleForUser(rankRoles, subject, out var nextRankRole, out _))
                return nextRankRole;

            return null;
        }

        /// <inheritdoc />
        public async Task<PromotionActionSummary> AddCommentAsync(long campaignId, PromotionSentiment sentiment, string? content)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsComment);

            if (await PromotionCommentRepository.AnyAsync(new PromotionCommentSearchCriteria()
            {
                CampaignId = campaignId,
                CreatedById = AuthorizationService.CurrentUserId.Value,
                IsModified = false
            }))
                throw new InvalidOperationException("Only one comment can be made per user, per campaign.");

            var campaign = await PromotionCampaignRepository.ReadDetailsAsync(campaignId);

            if (campaign is null)
                throw new InvalidOperationException($"Campaign {campaignId} could not be found.");

            if (campaign.Subject.Id == AuthorizationService.CurrentUserId)
                throw new InvalidOperationException("You aren't allowed to comment on your own campaign.");

            if (campaign.CloseAction is not null)
                throw new InvalidOperationException($"Campaign {campaignId} has already been closed.");

            var rankRoles = await GetRankRolesAsync(AuthorizationService.CurrentGuildId.Value);

            if (!await CheckIfUserIsRankOrHigherAsync(rankRoles, AuthorizationService.CurrentUserId.Value, campaign.TargetRole.Id))
                throw new InvalidOperationException("Commenting on a promotion campaign requires a rank at least as high as the proposed target rank.");

            PromotionActionSummary resultAction;

            using (var transaction = await PromotionCommentRepository.BeginCreateTransactionAsync())
            {
                resultAction = await PromotionCommentRepository.CreateAsync(new PromotionCommentCreationData()
                {
                    GuildId = campaign.GuildId,
                    CampaignId = campaignId,
                    Sentiment = sentiment,
                    Content = content,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                transaction.Commit();
            }

            PublishActionNotificationAsync(resultAction);

            return resultAction;
        }

        /// <inheritdoc />
        public async Task<PromotionActionSummary> UpdateCommentAsync(long commentId, PromotionSentiment newSentiment, string? newContent)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsComment);

            PromotionActionSummary resultAction;

            using (var transaction = await PromotionCommentRepository.BeginUpdateTransactionAsync())
            {
                var oldComment = await PromotionCommentRepository.ReadSummaryAsync(commentId);
                var campaign = await PromotionCampaignRepository.ReadDetailsAsync(oldComment!.Campaign!.Id);

                if (campaign!.CloseAction is not null)
                    throw new InvalidOperationException($"Campaign {oldComment.Campaign.Id} has already been closed.");

                resultAction = await PromotionCommentRepository.TryUpdateAsync(commentId, AuthorizationService.CurrentUserId.Value,
                    x =>
                    {
                        x.Sentiment = newSentiment;
                        x.Content = newContent;
                    });

                transaction.Commit();
            }

            PublishActionNotificationAsync(resultAction);

            return resultAction;
        }

        public async Task AddOrUpdateCommentAsync(long campaignId, Optional<PromotionSentiment> sentiment, Optional<string?> content = default)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsComment);

            using var transaction = await PromotionCommentRepository.BeginCreateTransactionAsync();

            var campaign = await PromotionCampaignRepository.ReadDetailsAsync(campaignId)
                ?? throw new InvalidOperationException($"Campaign {campaignId} could not be found.");

            if (campaign.CloseAction is not null)
                throw new InvalidOperationException($"Campaign {campaignId} has already been closed.");

            PromotionActionSummary resultAction;

            var existingComment = (await PromotionCommentRepository.SearchAsync(new()
            {
                CampaignId = campaignId,
                CreatedById = AuthorizationService.CurrentUserId.Value,
                IsModified = false,
            }))
            .FirstOrDefault();

            if (existingComment is null)
            {
                var rankRoles = await GetRankRolesAsync(AuthorizationService.CurrentGuildId.Value);

                if (!await CheckIfUserIsRankOrHigherAsync(rankRoles, AuthorizationService.CurrentUserId.Value, campaign.TargetRole.Id))
                    throw new InvalidOperationException("Commenting on a promotion campaign requires a rank at least as high as the proposed target rank.");

                if (!sentiment.IsSpecified)
                    throw new ArgumentException("A sentiment (approve or oppose) is required for new votes.");

                resultAction = await PromotionCommentRepository.CreateAsync(new PromotionCommentCreationData()
                {
                    GuildId = campaign.GuildId,
                    CampaignId = campaignId,
                    Sentiment = sentiment.GetValueOrDefault(),
                    Content = content.GetValueOrDefault(),
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });
            }
            else
            {
                // Don't update if nothing has changed.

                if (!sentiment.IsSpecified && !content.IsSpecified)
                    return;
                else if (sentiment.IsSpecified && content.IsSpecified)
                {
                    if (sentiment.Value == existingComment.Sentiment && content.Value == (existingComment.Content ?? string.Empty))
                        return;
                }
                else if (sentiment.IsSpecified && sentiment.Value == existingComment.Sentiment)
                    return;
                else if (content.IsSpecified && content.Value == (existingComment.Content ?? string.Empty))
                    return;

                resultAction = await PromotionCommentRepository.TryUpdateAsync(existingComment.Id, AuthorizationService.CurrentUserId.Value,
                    x =>
                    {
                        x.Sentiment = sentiment.IsSpecified
                            ? sentiment.Value
                            : existingComment.Sentiment;
                        x.Content = content.IsSpecified
                            ? content.Value
                            : existingComment.Content;
                    });
            }

            transaction.Commit();

            PublishActionNotificationAsync(resultAction);
        }

        /// <inheritdoc />
        public async Task AcceptCampaignAsync(long campaignId, bool force)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsCloseCampaign);

            PromotionActionSummary? resultAction;

            using (var transaction = await PromotionCampaignRepository.BeginCloseTransactionAsync())
            {
                var campaign = await PromotionCampaignRepository.ReadDetailsAsync(campaignId);
                if (campaign is null)
                    throw new InvalidOperationException($"Campaign {campaignId} does not exist.");

                if (campaign.CloseAction is not null)
                    throw new InvalidOperationException($"Campaign {campaignId} is already closed.");

                var timeSince = DateTime.UtcNow - campaign.CreateAction.Created;

                if (timeSince < PromotionCampaignEntityExtensions.CampaignAcceptCooldown && !force)
                    throw new InvalidOperationException($"Campaign {campaignId} cannot be accepted until {PromotionCampaignEntityExtensions.CampaignAcceptCooldown.TotalHours} hours after its creation ({(PromotionCampaignEntityExtensions.CampaignAcceptCooldown - timeSince).Humanize(4)} remain).");

                try
                {
                    var subject = await UserService.GetGuildUserAsync(campaign.GuildId, campaign.Subject.Id);
                    if (subject.RoleIds.Contains(campaign.TargetRole.Id))
                        throw new InvalidOperationException($"User {subject.GetDisplayName()} is already a member of role {campaign.TargetRole.Name}.");

                    var guild = await DiscordClient.GetGuildAsync(campaign.GuildId);
                    var targetRole = guild.GetRole(campaign.TargetRole.Id);
                    if (targetRole is null)
                        throw new InvalidOperationException($"Role {campaign.TargetRole.Name} no longer exists.");

                    await subject.AddRoleAsync(targetRole);

                    foreach (var lowerRankRole in (await GetRankRolesAsync(AuthorizationService.CurrentGuildId.Value))
                        .TakeWhile(x => x.Id != targetRole.Id))
                    {
                        var lowerRole = guild.GetRole(lowerRankRole.Id);
                        if (lowerRole is not null && subject.RoleIds.Contains(lowerRole.Id))
                            await subject.RemoveRoleAsync(lowerRole);
                    }

                    resultAction = await PromotionCampaignRepository.TryCloseAsync(campaignId, AuthorizationService.CurrentUserId.Value, PromotionCampaignOutcome.Accepted);
                }
                catch
                {
                    resultAction = await PromotionCampaignRepository.TryCloseAsync(campaignId, AuthorizationService.CurrentUserId.Value, PromotionCampaignOutcome.Failed);
                    PublishActionNotificationAsync(resultAction);
                    throw;
                }
                finally
                {
                    transaction.Commit();
                }
            }

            PublishActionNotificationAsync(resultAction);
        }

        /// <inheritdoc />
        public async Task RejectCampaignAsync(long campaignId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsCloseCampaign);

            if (!(await PromotionCampaignRepository.TryCloseAsync(campaignId, AuthorizationService.CurrentUserId.Value, PromotionCampaignOutcome.Rejected) is PromotionActionSummary resultAction))
                throw new InvalidOperationException($"Campaign {campaignId} doesn't exist or is already closed.");

            PublishActionNotificationAsync(resultAction);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PromotionCampaignSummary>> SearchCampaignsAsync(PromotionCampaignSearchCriteria searchCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsRead);

            return await PromotionCampaignRepository.SearchSummariesAsync(searchCriteria);
        }

        /// <inheritdoc />
        public async Task<PromotionCampaignDetails?> GetCampaignDetailsAsync(long campaignId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsRead);

            var result = await PromotionCampaignRepository.ReadDetailsAsync(campaignId);

            if (result?.Subject.Id == AuthorizationService.CurrentUserId)
            {
                throw new InvalidOperationException("You can't view comments on your own campaign.");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<PromotionActionSummary?> GetPromotionActionSummaryAsync(long promotionActionId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsRead);

            return await PromotionActionRepository.ReadSummaryAsync(promotionActionId);
        }

        public async Task<PromotionCommentSummary[]> SearchPromotionCommentsAsync(PromotionCommentSearchCriteria searchCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsRead);

            return await PromotionCommentRepository.SearchAsync(searchCriteria);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PromotionCampaignSummary>> GetPromotionsForUserAsync(ulong guildId, ulong userId)
            => await PromotionCampaignRepository.GetPromotionsForUserAsync(guildId, userId);

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleService"/> for interacting with roles explicitly designated for use by the application.
        /// </summary>
        internal protected IDesignatedRoleService DesignatedRoleService { get; }

        /// <summary>
        /// An <see cref="IPromotionActionRepository"/> for storing and retrieving promotion action data.
        /// </summary>
        internal protected IPromotionActionRepository PromotionActionRepository { get; }

        /// <summary>
        /// An <see cref="IPromotionCampaignRepository"/> for storing and retrieving promotion campaign data.
        /// </summary>
        internal protected IPromotionCampaignRepository PromotionCampaignRepository { get; }

        /// <summary>
        /// An <see cref="IPromotionCommentRepository"/> for storing and retrieving promotion comment data.
        /// </summary>
        internal protected IPromotionCommentRepository PromotionCommentRepository { get; }

        /// <summary>
        /// An <see cref="IMessageDispatcher"/> for dispatching notifications throughout the application.
        /// </summary>
        internal protected IMessageDispatcher MessageDispatcher { get; }

        private async Task<GuildRoleBrief[]> GetRankRolesAsync(ulong guildId)
            => (await DesignatedRoleService.SearchDesignatedRolesAsync(new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guildId,
                Type = DesignatedRoleType.Rank,
                IsDeleted = false
            }))
                .Select(x => x.Role)
                .OrderBy(x => x.Position)
                .ThenBy(x => x.Id)
                .ToArray();

        private async Task<bool> CheckIfUserIsRankOrHigherAsync(IEnumerable<GuildRoleBrief> rankRoles, ulong userId, ulong targetRoleId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            if (AuthorizationService.CurrentUserId.Value == DiscordClient.CurrentUser.Id)
                return true;

            var currentUser = await UserService.GetGuildUserAsync(AuthorizationService.CurrentGuildId.Value, AuthorizationService.CurrentUserId.Value);

            return rankRoles
                .Select(x => x.Id)
                .SkipWhile(x => x != targetRoleId)
                .Intersect(currentUser.RoleIds)
                .Any();
        }

        private void PublishActionNotificationAsync(PromotionActionSummary? action)
        {
            if (action is null)
                return;

            MessageDispatcher.Dispatch(new PromotionActionCreatedNotification(
                        action.Id,
                        new PromotionActionCreationData
                        {
                            Created = action.Created,
                            CreatedById = action.CreatedBy.Id,
                            GuildId = action.GuildId,
                            Type = action.Type,
                        }));
        }

        private async Task FinalizeCreateCampaignAsync(ulong subjectId, ulong targetRoleId, string? comment)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            PromotionActionSummary campaignResultAction;
            PromotionActionSummary commentResultAction;

            using (var campaignTransaction = await PromotionCampaignRepository.BeginCreateTransactionAsync())
            using (var commentTransaction = await PromotionCommentRepository.BeginCreateTransactionAsync())
            {
                campaignResultAction = await PromotionCampaignRepository.CreateAsync(new PromotionCampaignCreationData()
                {
                    GuildId = AuthorizationService.CurrentGuildId.Value,
                    SubjectId = subjectId,
                    TargetRoleId = targetRoleId,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                commentResultAction = await PromotionCommentRepository.CreateAsync(new PromotionCommentCreationData()
                {
                    GuildId = AuthorizationService.CurrentGuildId.Value,
                    CampaignId = campaignResultAction.Campaign!.Id,
                    Sentiment = PromotionSentiment.Approve,
                    Content = comment,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                campaignTransaction.Commit();
                commentTransaction.Commit();
            }

            PublishActionNotificationAsync(campaignResultAction);
            PublishActionNotificationAsync(commentResultAction);
        }

        private async Task PerformCommonCreateCampaignValidationsAsync(IGuildUser subject, GuildRoleBrief targetRankRole, IEnumerable<GuildRoleBrief> rankRoles)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            if (await PromotionCampaignRepository.AnyAsync(new PromotionCampaignSearchCriteria()
            {
                GuildId = AuthorizationService.CurrentGuildId.Value,
                SubjectId = subject.Id,
                TargetRoleId = targetRankRole.Id,
                IsClosed = false
            }))
                throw new InvalidOperationException($"An active campaign already exists for {subject.GetDisplayName()} to be promoted to {targetRankRole.Name}.");

            // JoinedAt is null, when it cannot be obtained
            if (subject.JoinedAt.HasValue)
                if (subject.JoinedAt.Value.DateTime > (DateTimeOffset.UtcNow - TimeSpan.FromDays(20)))
                    throw new InvalidOperationException($"{subject.GetDisplayName()} has joined less than 20 days prior.");

            if (!await CheckIfUserIsRankOrHigherAsync(rankRoles, AuthorizationService.CurrentUserId.Value, targetRankRole.Id))
                throw new InvalidOperationException($"Creating a promotion campaign requires a rank at least as high as the proposed target rank.");
        }

        private bool TryGetNextRankRoleForUser(
            GuildRoleBrief[] rankRoles, IGuildUser subject, [NotNullWhen(true)] out GuildRoleBrief? nextRankRole, out string? message)
        {
            var userRankRoles = rankRoles.Where(r => subject.RoleIds.Contains(r.Id));
            var userCurrentRankRole = userRankRoles.OrderByDescending(r => r.Position).FirstOrDefault();

            if (userCurrentRankRole is null)
            {
                nextRankRole = rankRoles.OrderBy(r => r.Position).FirstOrDefault();

                if (nextRankRole is null)
                {
                    message = "There are no rank roles defined.";
                    return false;
                }
                else
                {
                    message = null;
                    return true;
                }
            }
            else
            {
                nextRankRole = rankRoles.OrderBy(x => x.Position).FirstOrDefault(r => r.Position > userCurrentRankRole.Position);

                if (nextRankRole is null)
                {
                    message = $"There are no rank roles available for {subject.GetDisplayName()} to be promoted to.";
                    return false;
                }
                else
                {
                    message = null;
                    return true;
                }
            }
        }

        private void ValidateCreateCampaignAuthorization()
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.PromotionsCreateCampaign);
        }
    }
}
