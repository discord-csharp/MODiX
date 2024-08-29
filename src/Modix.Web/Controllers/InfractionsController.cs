using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;
using Modix.Web.Shared.Models.Infractions;
using MudBlazor;

namespace Modix.Web.Controllers;

[Route("~/api/infractions")]
[ApiController]
[Authorize]
public class InfractionsController : ModixController
{
    private readonly ModerationService _moderationService;

    public InfractionsController(ModerationService moderationService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _moderationService = moderationService;
    }

    // TODO: Refactor this
    public class Intermediate
    {
        public TableFilter tableFilter { get; set; }
        public TableState tableState { get; set; }
    }

    [HttpPut]
    [Authorize(Roles = nameof(AuthorizationClaim.ModerationRead))]
    public async Task<InfractionData[]> GetInfractionsAsync(Intermediate inter)
    {
        var tableState = inter.tableState;
        var tableFilter = inter.tableFilter;

        var sortingCriteria = new[]
        {
            new SortingCriteria
            {
                PropertyName = tableState.SortLabel ?? nameof(InfractionData.Id),
                Direction = tableState.SortDirection == MudBlazor.SortDirection.Ascending
                    ? Data.Models.SortDirection.Ascending
                    : Data.Models.SortDirection.Descending,
            }
        };

        var searchCriteria = new InfractionSearchCriteria
        {
            GuildId = UserGuild.Id,
            Id = tableFilter.Id,
            Types = tableFilter.Types?.Cast<Data.Models.Moderation.InfractionType>().ToArray(),
            Subject = tableFilter.Subject,
            SubjectId = tableFilter.SubjectId,
            Creator = tableFilter.Creator,
            CreatedById = tableFilter.CreatedById,
            IsDeleted = tableFilter.ShowDeleted ? null : false
        };

        var pagingCriteria = new PagingCriteria
        {
            FirstRecordIndex = tableState.Page * tableState.PageSize,
            PageSize = tableState.PageSize,
        };

        var infractions = await _moderationService.SearchInfractionsAsync(searchCriteria, sortingCriteria, pagingCriteria);
        var outranksValues = new Dictionary<ulong, bool>();

        foreach (var (guildId, subjectId) in infractions.Records.Select(x => (guildId: x.GuildId, subjectId: x.Subject.Id)))
        {
            outranksValues[subjectId] = await _moderationService.DoesModeratorOutrankUserAsync(guildId, SocketUser.Id, subjectId);
        }

        return infractions.Records
            .Select(x => new InfractionData(
                x.Id,
                x.GuildId,
                // TODO:
                (Shared.Models.Infractions.InfractionType)(int)x.Type,
                x.Reason,
                x.Duration,
                x.Subject.Username,
                x.CreateAction.CreatedBy.Username,
                x.CreateAction.Created,

                x.RescindAction is not null,
                x.DeleteAction is not null,

                x.RescindAction is null
                    && x.DeleteAction is null
                    && (x.Type == Data.Models.Moderation.InfractionType.Mute || x.Type == Data.Models.Moderation.InfractionType.Ban)
                    && outranksValues[x.Subject.Id],

                x.DeleteAction is null
                    && outranksValues[x.Subject.Id]
                ))
            .ToArray();
    }

    [Authorize(Roles = nameof(AuthorizationClaim.ModerationRescind))]
    [HttpPost("rescind/{infractionId}")]
    public async Task RescindInfractionAsync(long infractionId)
    {
        await _moderationService.RescindInfractionAsync(infractionId);
    }

    [Authorize(Roles = nameof(AuthorizationClaim.ModerationDeleteInfraction))]
    [HttpDelete("{infractionId}")]
    public async Task DeleteInfractionAsync(long infractionId)
    {
        await _moderationService.DeleteInfractionAsync(infractionId);
    }

    [Authorize(Roles = $"{nameof(AuthorizationClaim.ModerationNote)},{nameof(AuthorizationClaim.ModerationWarn)},{nameof(AuthorizationClaim.ModerationBan)},{nameof(AuthorizationClaim.ModerationMute)}")]
    [HttpPost("create")]
    public async Task CreateInfractionAsync(Shared.Models.Infractions.InfractionCreationData creationData)
    {
        await _moderationService.CreateInfractionAsync(
            UserGuild.Id,
            SocketUser.Id,
            // TODO:
            (Data.Models.Moderation.InfractionType)(int)creationData.Type,
            creationData.SubjectId,
            creationData.Reason,
            creationData.Duration);
    }
}
