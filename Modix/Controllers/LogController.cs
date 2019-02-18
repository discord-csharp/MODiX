using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Newtonsoft.Json;

namespace Modix.Controllers
{
    [Route("~/api/logs")]
    public class LogController : ModixController
    {
        private IModerationService ModerationService { get; }

        public LogController(DiscordSocketClient client, IAuthorizationService modixAuth, IModerationService moderationService) : base(client, modixAuth)
        {
            ModerationService = moderationService;
        }

        [HttpPut("deletedMessages")]
        public async Task<IActionResult> DeletedMessagesAsync()
        {
            var buffer = new byte[Request.ContentLength.Value];
            await Request.Body.ReadAsync(buffer, 0, (int)Request.ContentLength);
            var json = Encoding.UTF8.GetString(buffer);
            var tableParams = JsonConvert.DeserializeObject<TableParameters>(json);

            var sortProperty = DeletedMessageSummary.SortablePropertyNames.FirstOrDefault(
                x => x.Equals(tableParams.Sort.Field, StringComparison.OrdinalIgnoreCase)) ?? nameof(DeletedMessageSummary.Created);

            var searchCriteria = new DeletedMessageSearchCriteria() { GuildId = UserGuild.Id };

            foreach (var filter in tableParams.Filters)
            {
                searchCriteria.WithPropertyValue(filter.Field, filter.Value);
            }

            var result = await ModerationService.SearchDeletedMessagesAsync(searchCriteria,
            new[]
            {
                new SortingCriteria
                {
                    PropertyName = sortProperty,
                    Direction = tableParams.Sort.Direction,
                }
            },
            new PagingCriteria
            {
                FirstRecordIndex = tableParams.Page * tableParams.PerPage,
                PageSize = tableParams.PerPage,
            });

            var mapped = new
            {
                result.TotalRecordCount,
                result.FilteredRecordCount,
                Records = result.Records.Select(
                    x => new
                    {
                        Channel = x.Channel.Name,
                        Author = x.Author.DisplayName,
                        x.Created,
                        CreatedBy = x.CreatedBy.DisplayName,
                        x.Content,
                        x.Reason,
                        x.BatchId,
                    }),
            };

            return Ok(mapped);
        }
    }
}
