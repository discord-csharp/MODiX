using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<QueryPage<InfractionEntity>> FindInfractionsAsync(InfractionSearchCriteria searchCriteria, PagingCriteria pagingCriteria)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchAsync(searchCriteria, pagingCriteria);
        }

        public async Task RecordInfractionAsync(InfractionType type, long subjectId, string reason, TimeSpan? duration)
        {
            await AuthorizationService.RequireClaimsAsync(_recordInfractionClaimsByType[type]);

            var infractionId = await InfractionRepository.InsertAsync(new InfractionEntity()
            {
                Type = type,
                SubjectId = subjectId,

                Reason = reason,
                Duration = duration
            });

            await CreateModerationActionAsync(infractionId, ModerationActionType.InfractionCreated, reason);
        }

        public async Task RescindInfractionAsync(long infractionId, string comment)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaim.ModerationRescind);

            await InfractionRepository.UpdateIsRescindedAsync(infractionId, AuthenticationService.CurrentUserId.Value);

            await CreateModerationActionAsync(infractionId, ModerationActionType.InfractionModified, comment);
        }

        public event EventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        private async Task<long> CreateModerationActionAsync(long infractionId, ModerationActionType type, string comment)
        {
            var action = new ModerationActionEntity()
            {
                InfractionId = infractionId,
                Type = type,
                CreatedById = AuthenticationService.CurrentUserId.Value,
                Comment = comment
            };

            var actionId = await ModerationActionRepository.InsertAsync(action);

            ModerationActionCreated?.Invoke(this, new ModerationActionCreatedEventArgs(action));

            return actionId;
        }

        protected internal IAuthenticationService AuthenticationService { get; }

        protected internal IAuthorizationService AuthorizationService { get; }

        protected internal IInfractionRepository InfractionRepository { get; }

        protected internal IModerationActionRepository ModerationActionRepository { get; }

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _recordInfractionClaimsByType
            = new Dictionary<InfractionType, AuthorizationClaim>()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote },
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn },
                {InfractionType.Mute, AuthorizationClaim.ModerationMute },
                {InfractionType.Ban, AuthorizationClaim.ModerationBan }
            };
    }
}
