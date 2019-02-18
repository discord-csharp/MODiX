using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Mappings;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.RowboatImporter;

namespace Modix.Controllers
{
    [Route("~/api/infractions")]
    public class InfractionController : ModixController
    {
        private IModerationService ModerationService { get; }
        private RowboatInfractionImporterService ImporterService { get; }

        public InfractionController(DiscordSocketClient client, IAuthorizationService modixAuth, IModerationService moderationService, RowboatInfractionImporterService importerService) : base(client, modixAuth)
        {
            ModerationService = moderationService;
            ImporterService = importerService;
        }

        [HttpPut]
        public async Task<IActionResult> GetInfractionsAsync([FromBody] TableParameters tableParams)
        {
            var sortingCriteria = tableParams.Sort.ToInfractionSummarySortingCriteria();

            var searchCriteria = tableParams.Filters.ToInfractionSearchCriteria();
            searchCriteria.GuildId = UserGuild.Id;

            var pagingCriteria = tableParams.ToPagingCriteria();

            var result = await ModerationService.SearchInfractionsAsync(
                searchCriteria,
                sortingCriteria,
                pagingCriteria);

            var outranksValues = new Dictionary<ulong, bool>();

            foreach (var (guildId, subjectId) in result.Records
                .Select(x => (guildId: x.GuildId, subjectId: x.Subject.Id))
                .Distinct())
            {
                outranksValues[subjectId]
                    = await ModerationService.DoesModeratorOutrankUserAsync(guildId, SocketUser.Id, subjectId);
            }

            var mapped = result.Records.Select(
                x => new InfractionData
                {
                    Id = x.Id,
                    GuildId = x.GuildId,
                    Type = x.Type,
                    Reason = x.Reason,
                    Duration = x.Duration,
                    Subject = x.Subject,

                    CreateAction = x.CreateAction,
                    RescindAction = x.RescindAction,
                    DeleteAction = x.DeleteAction,

                    CanRescind
                        = x.RescindAction is null
                        && x.DeleteAction is null
                        && (x.Type == InfractionType.Mute || x.Type == InfractionType.Ban)
                        && outranksValues[x.Subject.Id],

                    CanDelete
                        = x.DeleteAction is null
                        && outranksValues[x.Subject.Id],
                }).ToArray();

            return Ok(new RecordsPage<InfractionData>()
            {
                FilteredRecordCount = result.FilteredRecordCount,
                TotalRecordCount = result.TotalRecordCount,
                Records = mapped,
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> InfractionsForUserAsync(ulong id)
        {
            var result = await ModerationService.SearchInfractionsAsync(new InfractionSearchCriteria
            {
                IsDeleted = false,
                IsRescinded = false,
                SubjectId = id,
                GuildId = UserGuild.Id
            });

            return Ok(result);
        }

        [HttpPut("{subjectId}/create")]
        public async Task<IActionResult> CreateInfractionAsync(ulong subjectId, [FromBody] Models.InfractionCreationData creationData)
        {
            if (!Enum.TryParse<InfractionType>(creationData.Type, out var type))
                return BadRequest($"{creationData.Type} is not a valid infraction type.");

            var duration = GetTimeSpan(
                creationData.DurationMonths,
                creationData.DurationDays,
                creationData.DurationHours,
                creationData.DurationMinutes,
                creationData.DurationSeconds);

            try
            {
                await ModerationService.CreateInfractionAsync(type, subjectId, creationData.Reason, duration);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

            TimeSpan? GetTimeSpan(int? months, int? days, int? hours, int? minutes, int? seconds)
            {
                if (months is null
                    && days is null
                    && hours is null
                    && minutes is null
                    && seconds is null)
                    return null;
                
                var now = DateTimeOffset.UtcNow;
                var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

                var monthSpan = months is null
                    ? TimeSpan.Zero
                    : TimeSpan.FromDays(months.Value * daysInMonth);

                var daySpan = days is null
                    ? TimeSpan.Zero
                    : TimeSpan.FromDays(days.Value);

                var hourSpan = hours is null
                    ? TimeSpan.Zero
                    : TimeSpan.FromHours(hours.Value);

                var minuteSpan = minutes is null
                    ? TimeSpan.Zero
                    : TimeSpan.FromMinutes(minutes.Value);

                var secondSpan = seconds is null
                    ? TimeSpan.Zero
                    : TimeSpan.FromSeconds(seconds.Value);

                return monthSpan + daySpan + hourSpan + minuteSpan + secondSpan;
            }
        }

        [HttpPost("{id}/rescind")]
        public async Task<IActionResult> RescindInfractionAsync(long id)
        {
            try
            {
                await ModerationService.RescindInfractionAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> DeleteInfractionAsync(long id)
        {
            try
            {
                await ModerationService.DeleteInfractionAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("{subjectId}/doesModeratorOutrankUser")]
        public async Task<IActionResult> DoesModeratorOutrankUserAsync(ulong subjectId)
            => Ok(await ModerationService.DoesModeratorOutrankUserAsync(ModixAuth.CurrentGuildId.Value, ModixAuth.CurrentUserId.Value, subjectId));

        [HttpPut("import")]
        public Task<IActionResult> Import()
        {
            throw new NotImplementedException();

            //if (Request.Form.Files.Count == 0 || Request.Form.Files.First().ContentType != "application/json")
            //{
            //    return BadRequest("Must submit a JSON file");
            //}

            //int importCount = 0;

            //try
            //{
            //    using (var httpStream = Request.Form.Files.First().OpenReadStream())
            //    using (var streamReader = new StreamReader(httpStream))
            //    {
            //        var content = await streamReader.ReadToEndAsync();
            //        var loaded = JsonConvert.DeserializeObject<IEnumerable<RowboatInfraction>>(content);

            //        importCount = await ImporterService.ImportInfractionsAsync(loaded);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(ex.Message);
            //}

            //return Ok(importCount);
        }
    }
}
