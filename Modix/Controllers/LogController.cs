using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;
using Modix.Services.Moderation;

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

        [HttpGet("{pageSize}/{pageNumber}/deletedMessages")]
        public async Task<IActionResult> DeletedMessagesAsync(int pageSize, int pageNumber)
        {
            var result = await ModerationService.SearchDeletedMessagesAsync(new DeletedMessageSearchCriteria()
            {
                GuildId = UserGuild.Id
            },
            new[]
            {
                new SortingCriteria
                {
                    PropertyName = nameof(DeletedMessageSummary.Created),
                    Direction = SortDirection.Descending,
                }
            },
            new PagingCriteria
            {
                FirstRecordIndex = pageNumber * pageSize,
                PageSize = pageSize,
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

            return Ok(result);
        }
    }
}
