using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Models;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Controllers
{
    [ApiController]
    [Route("~/api/logs")]
    public class LogController : ModixController
    {
        private readonly IModerationService _moderationService;

        public LogController(DiscordSocketClient client, IAuthorizationService modixAuth, IModerationService moderationService) : base(client, modixAuth)
        {
            _moderationService = moderationService;
        }

        [HttpGet("deletedMessages/context/{batchId}")]
        public async Task<IActionResult> GetDeletionContextAsync(long batchId)
        {
            var deletedMessages = await _moderationService.SearchDeletedMessagesAsync(new DeletedMessageSearchCriteria
            {
                BatchId = batchId
            },
            new[]
            {
                //Sort ascending, so the earliest message is first
                new SortingCriteria { PropertyName = nameof(DeletedMessageSummary.MessageId), Direction = SortDirection.Ascending }
            },
            new PagingCriteria());

            var firstMessage = deletedMessages.Records.FirstOrDefault();

            if (firstMessage == null)
            {
                return NotFound($"Couldn't find messages for batch id {batchId}");
            }

            var batchChannelId = deletedMessages.Records.First().Channel.Id;

            if (!(UserGuild.GetChannel(batchChannelId) is ISocketMessageChannel foundChannel))
            {
                return NotFound($"Couldn't recreate context - text channel with id {batchChannelId} not found");
            }

            if (SocketUser.GetPermissions(foundChannel as IGuildChannel).ReadMessageHistory == false)
            {
                return Unauthorized($"You don't have read permissions for the channel this batch was deleted in (#{foundChannel.Name})");
            }

            var beforeMessages = await foundChannel.GetMessagesAsync(firstMessage.MessageId, Direction.Before, 25).FlattenAsync();
            var afterMessages = await foundChannel.GetMessagesAsync(firstMessage.MessageId, Direction.After, 25 + (int)deletedMessages.FilteredRecordCount).FlattenAsync();

            var allMessages = new List<DeletedMessageAbstraction>();
            allMessages.AddRange(deletedMessages.Records.Select(d => DeletedMessageAbstraction.FromSummary(d)));
            allMessages.AddRange(beforeMessages.Select(d => DeletedMessageAbstraction.FromIMessage(d)));
            allMessages.AddRange(afterMessages.Select(d => DeletedMessageAbstraction.FromIMessage(d)));

            var sorted = allMessages.OrderBy(d => d.MessageId);

            return Ok(sorted);
        }

        [HttpPut("deletedMessages")]
        public async Task<IActionResult> DeletedMessagesAsync(TableParameters tableParams)
        {
            var sortProperty = DeletedMessageSummary.SortablePropertyNames.FirstOrDefault(
                x => x.Equals(tableParams.Sort.Field, StringComparison.OrdinalIgnoreCase)) ?? nameof(DeletedMessageSummary.Created);

            var searchCriteria = new DeletedMessageSearchCriteria() { GuildId = UserGuild.Id };

            foreach (var filter in tableParams.Filters)
            {
                searchCriteria.WithPropertyValue(filter.Field, filter.Value);
            }

            var result = await _moderationService.SearchDeletedMessagesAsync(searchCriteria,
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
                        Author = x.Author.GetFullUsername(),
                        x.Created,
                        CreatedBy = x.CreatedBy.GetFullUsername(),
                        x.Content,
                        x.Reason,
                        x.BatchId,
                    }),
            };

            return Ok(mapped);
        }
    }
}
