using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AsyncEvent;

using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

using Modix.Services.Authentication;
using Modix.Services.Authorization;

namespace Modix.Services.Moderation
{
    /// <inheritdoc />
    public class ModerationService : IModerationService
    {
        /// <summary>
        /// Creates a new <see cref="ModerationService"/>.
        /// </summary>
        /// <param name="authenticationService">The value to use for <see cref="AuthenticationService"/>.</param>
        /// <param name="authorizationService">The value to use for <see cref="AuthorizationService"/>.</param>
        /// <param name="infractionRepository">The value to use for <see cref="InfractionRepository"/>.</param>
        /// <param name="moderationActionRepository">The value to use for <see cref="ModerationActionRepository"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Throws for <paramref name="authenticationService"/>,
        /// <paramref name="authorizationService"/>,
        /// <paramref name="infractionRepository"/>,
        /// and <paramref name="moderationActionRepository"/>.
        /// </exception>
        public ModerationService(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IInfractionRepository infractionRepository, IModerationActionRepository moderationActionRepository)
        {
            AuthenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            InfractionRepository = infractionRepository ?? throw new ArgumentNullException(nameof(infractionRepository));
            ModerationActionRepository = moderationActionRepository ?? throw new ArgumentNullException(nameof(moderationActionRepository));
        }

        /// <inheritdoc />
        public async Task CreateInfractionAsync(InfractionType type, long subjectId, string reason, TimeSpan? duration)
        {
            await AuthorizationService.RequireClaimsAsync(_createInfractionClaimsByType[type]);

            // TODO: Check whether subjectId exists.

            // TODO: Perform muting/banning with Discord.NET

            var actionId = await ModerationActionRepository.InsertAsync(new ModerationActionData()
            {
                Type = ModerationActionType.InfractionCreated,
                CreatedById = AuthenticationService.CurrentUserId.Value,
                Reason = reason
            });

            var infractionId = await InfractionRepository.InsertAsync(new InfractionData()
            {
                Type = type,
                SubjectId = subjectId,
                Duration = duration,
                CreateActionId = actionId
            });

            await ModerationActionRepository.SetInfractionAsync(actionId, infractionId);

            await RaiseModerationActionCreatedAsync(actionId);
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(long infractionId, string reason)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRescind);

            if (!await InfractionRepository.ExistsAsync(infractionId))
                throw new ArgumentException("Infraction does not exist", nameof(infractionId));

            // TODO: Perform unmuting/unbanning with Discord.NET

            var actionId = await ModerationActionRepository.InsertAsync(new ModerationActionData()
            {
                Type = ModerationActionType.InfractionRescinded,
                CreatedById = AuthenticationService.CurrentUserId.Value,
                Reason = reason,
                InfractionId = infractionId
            });

            await RaiseModerationActionCreatedAsync(actionId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchAsync(searchCriteria, sortingCriteria);
        }

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchAsync(searchCriteria, sortingCriteria, pagingCriteria);
        }

        /// <inheritdoc />
        public event AsyncEventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        /// <summary>
        /// An <see cref="IAuthenticationService"/> for interacting with the current authenticated user, within the application.
        /// </summary>
        internal protected IAuthenticationService AuthenticationService { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> for interacting with the permissions of the current user, within the application.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IInfractionRepository"/> for storing and retrieving infraction data.
        /// </summary>
        internal protected IInfractionRepository InfractionRepository { get; }

        /// <summary>
        /// An <see cref="IModerationActionRepository"/> for storing and retrieving moderation action data.
        /// </summary>
        internal protected IModerationActionRepository ModerationActionRepository { get; }

        internal protected async Task RaiseModerationActionCreatedAsync(long actionId)
            => await ModerationActionCreated.InvokeAsync(
                this,
                new ModerationActionCreatedEventArgs(
                    await ModerationActionRepository.GetAsync(actionId)));

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
            = new Dictionary<InfractionType, AuthorizationClaim>()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote },
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn },
                {InfractionType.Mute, AuthorizationClaim.ModerationMute },
                {InfractionType.Ban, AuthorizationClaim.ModerationBan }
            };
    }
}
