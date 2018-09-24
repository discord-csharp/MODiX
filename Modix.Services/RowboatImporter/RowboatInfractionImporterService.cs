using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Moderation;

namespace Modix.Services.RowboatImporter
{
    public class RowboatInfractionImporterService
    {
        private IGuildUserRepository GuildUserRepository { get; }
        private IInfractionRepository InfractionRepository { get; }
        private IAuthorizationService AuthorizationService { get; }

        public RowboatInfractionImporterService(IGuildUserRepository guildUserRepository, IInfractionRepository infractionRepository, IAuthorizationService authorizationService)
        {
            GuildUserRepository = guildUserRepository;
            InfractionRepository = infractionRepository;
            AuthorizationService = authorizationService;
        }

        /// <summary>
        /// Imports the given <see cref="RowboatInfraction"/>s, mapping them to Modix infractions
        /// </summary>
        /// <param name="rowboatInfractions">The <see cref="IEnumerable{T}"/> of infractions to be imported</param>
        /// <returns>The count of imported infractions</returns>
        public async Task<int> ImportInfractionsAsync(IEnumerable<RowboatInfraction> rowboatInfractions)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationConfigure, AuthorizationClaim.ModerationWarn, 
                AuthorizationClaim.ModerationNote, AuthorizationClaim.ModerationBan);

            if (!AuthorizationService.CurrentGuildId.HasValue)
            {
                throw new InvalidOperationException("Cannot import infractions without a guild context");
            }

            int importCount = 0;

            using (var transaction = await InfractionRepository.BeginCreateTransactionAsync())
            {
                foreach (var infraction in rowboatInfractions.Where(d=>d.Active))
                {
                    if (await GuildUserRepository.ReadSummaryAsync(infraction.User.Id, AuthorizationService.CurrentGuildId.Value) != null &&
                        await GuildUserRepository.ReadSummaryAsync(infraction.Actor.Id, AuthorizationService.CurrentGuildId.Value) != null)
                    {
                        await InfractionRepository.CreateAsync(
                            new InfractionCreationData()
                            {
                                GuildId = AuthorizationService.CurrentGuildId.Value,
                                Type = infraction.ModixInfractionType,
                                SubjectId = infraction.User.Id,
                                Reason = infraction.Reason,
                                CreatedById = infraction.Actor.Id
                            });

                        importCount++;
                    }
                }

                transaction.Commit();
            }

            return importCount;
        }
    }
}
