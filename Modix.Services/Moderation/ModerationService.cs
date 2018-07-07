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
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaims.ModerationRead);

            return await InfractionRepository.SearchAsync(searchCriteria, pagingCriteria);
        }

        public async Task RecordInfractionAsync(InfractionTypes type, long subjectId, string reason, TimeSpan? duration)
        {
            await AuthorizationService.RequireClaimsAsync(_recordInfractionClaimsByType[type]);

            var infractionId = await InfractionRepository.InsertAsync(new InfractionEntity()
            {
                Type = type,
                SubjectId = subjectId,

                Reason = reason,
                Duration = duration
            });

            await CreateModerationActionAsync(infractionId, ModerationActionTypes.InfractionCreated, reason);
        }

        public async Task RescindInfractionAsync(long infractionId, string comment)
        {
            await AuthorizationService.RequireClaimsAsync(AuthorizationClaims.ModerationRescind);

            await InfractionRepository.UpdateIsRescindedAsync(infractionId, true);

            await CreateModerationActionAsync(infractionId, ModerationActionTypes.InfractionModified, comment);
        }

        public event EventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        private async Task<long> CreateModerationActionAsync(long infractionId, ModerationActionTypes type, string comment)
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

        internal protected IAuthenticationService AuthenticationService { get; }

        internal protected IAuthorizationService AuthorizationService { get; }

        internal protected IInfractionRepository InfractionRepository { get; }

        internal protected IModerationActionRepository ModerationActionRepository { get; }

        private static readonly Dictionary<InfractionTypes, AuthorizationClaims> _recordInfractionClaimsByType
            = new Dictionary<InfractionTypes, AuthorizationClaims>()
            {
                {InfractionTypes.Notice, AuthorizationClaims.ModerationNote },
                {InfractionTypes.Warning, AuthorizationClaims.ModerationWarn },
                {InfractionTypes.Mute, AuthorizationClaims.ModerationMute },
                {InfractionTypes.Ban, AuthorizationClaims.ModerationBan }
            };
    }
}
