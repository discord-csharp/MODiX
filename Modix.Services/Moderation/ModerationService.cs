using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

using Modix.Services.Authentication;
using Modix.Services.Authorization;

namespace Modix.Services.Moderation
{
    public class ModerationService : IModerationService
    {
        public ModerationService(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IInfractionRepository infractionRepository, IModerationActionRepository moderationActionRepository)
        {
            AuthenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            InfractionRepository = infractionRepository ?? throw new ArgumentNullException(nameof(infractionRepository));
            ModerationActionRepository = moderationActionRepository ?? throw new ArgumentNullException(nameof(moderationActionRepository));
        }

        public async Task<RecordsPage<InfractionSearchResult>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchAsync(searchCriteria, sortingCriteria, pagingCriteria);
        }

        public async Task CreateInfractionAsync(InfractionType type, long subjectId, string reason, TimeSpan? duration)
        {
            await AuthorizationService.RequireClaimsAsync(_createInfractionClaimsByType[type]);

            var infractionId = await InfractionRepository.InsertAsync(new InfractionEntity()
            {
                Type = type,
                SubjectId = subjectId,
                Duration = duration
            });

            await CreateModerationActionAsync(infractionId, ModerationActionType.InfractionCreated, reason);
        }

        public async Task RescindInfractionAsync(long infractionId, string reason)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRescind);

            await CreateModerationActionAsync(infractionId, ModerationActionType.InfractionRescinded, reason);
        }

        public event EventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        private async Task<long> CreateModerationActionAsync(long infractionId, ModerationActionType type, string reason)
        {
            var action = new ModerationActionEntity()
            {
                InfractionId = infractionId,
                Type = type,
                CreatedById = AuthenticationService.CurrentUserId.Value,
                Reason = reason
            };

            var actionId = await ModerationActionRepository.InsertAsync(action);

            ModerationActionCreated?.Invoke(this, new ModerationActionCreatedEventArgs(action));

            return actionId;
        }

        internal protected IAuthenticationService AuthenticationService { get; }

        internal protected IAuthorizationService AuthorizationService { get; }

        internal protected IInfractionRepository InfractionRepository { get; }

        internal protected IModerationActionRepository ModerationActionRepository { get; }

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
