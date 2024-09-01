using Discord;
using Discord.WebSocket;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Models.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;
using Modix.Web.Shared.Models.DeletedMessages;
using MudBlazor;

namespace Modix.Web.Controllers;

[Route("~/api/deletedmessages")]
[ApiController]
[Authorize(Roles = nameof(AuthorizationClaim.LogViewDeletedMessages))]
public class DeletedMessagesController : ModixController
{
    private readonly ModerationService _moderationService;

    public DeletedMessagesController(ModerationService moderationService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
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
    public async Task<DeletedMessageBatchInformation[]> GetDeletedMessagesBatchAsync(Intermediate inter)
    {
        var tableState = inter.tableState;
        var tableFilter = inter.tableFilter;

        var searchCriteria = new DeletedMessageSearchCriteria
        {
            GuildId = UserGuild.Id,
            Channel = tableFilter.Channel,
            ChannelId = tableFilter.ChannelId,
            Author = tableFilter.Author,
            AuthorId = tableFilter.AuthorId,
            CreatedBy = tableFilter.CreatedBy,
            CreatedById = tableFilter.CreatedById,
            Content = tableFilter.Content,
            Reason = tableFilter.Reason,
            BatchId = tableFilter.BatchId
        };

        var sortingCriteria = new SortingCriteria
        {
            PropertyName = tableState.SortLabel ?? nameof(DeletedMessageSummary.Created),
            Direction = tableState.SortDirection == MudBlazor.SortDirection.Ascending
                ? Data.Models.SortDirection.Ascending
                : Data.Models.SortDirection.Descending
        };

        var pagingCriteria = new PagingCriteria
        {
            FirstRecordIndex = tableState.Page * tableState.PageSize,
            PageSize = tableState.PageSize,
        };


        var deletedMessages = await _moderationService.SearchDeletedMessagesAsync(searchCriteria, [sortingCriteria], pagingCriteria);

        return deletedMessages.Records
            .Select(x => new DeletedMessageBatchInformation(
                x.Channel.Name,
                x.Author.GetFullUsername(),
                x.Created,
                x.CreatedBy.GetFullUsername(),
                x.Content,
                x.Reason,
                x.BatchId))
            .ToArray();
    }

    [HttpGet("{batchId}")]
    public async Task<IActionResult> GetDeletedMessageContext(long batchId)
    {

        var searchCriteria = new DeletedMessageSearchCriteria
        {
            BatchId = batchId
        };

        var sortingCriteria = new SortingCriteria
        {
            PropertyName = nameof(DeletedMessageSummary.MessageId),

            //Sort ascending, so the earliest message is first
            Direction = Data.Models.SortDirection.Ascending
        };

        var deletedMessages = await _moderationService.SearchDeletedMessagesAsync(searchCriteria, [sortingCriteria], new());
        var firstMessage = deletedMessages.Records.FirstOrDefault();

        if (firstMessage is null)
            return NotFound($"Couldn't find messages for batch id {batchId}");

        var batchChannelId = firstMessage.Channel.Id;
        if (SocketUser.Guild.GetChannel(batchChannelId) is not ISocketMessageChannel foundChannel)
            return NotFound($"Couldn't recreate context - text channel with id {batchChannelId} not found");

        if (!SocketUser.GetPermissions(foundChannel as IGuildChannel).ReadMessageHistory)
            return Unauthorized($"You don't have read permissions for the channel this batch was deleted in (#{foundChannel.Name})");

        var beforeMessages = await foundChannel.GetMessagesAsync(firstMessage.MessageId, Discord.Direction.Before, 25).FlattenAsync();
        var afterMessages = await foundChannel.GetMessagesAsync(firstMessage.MessageId, Discord.Direction.After, 25 + (int)deletedMessages.FilteredRecordCount).FlattenAsync();

        var allMessages = new List<DeletedMessageInformation>();
        allMessages.AddRange(deletedMessages.Records.Select(d => new DeletedMessageInformation(d.MessageId, null, null, d.Author.GetFullUsername(), d.Content)));
        allMessages.AddRange(beforeMessages.Select(FromIMessage));
        allMessages.AddRange(afterMessages.Select(FromIMessage));

        return Ok(allMessages);
    }

    private static DeletedMessageInformation FromIMessage(IMessage message)
    {
        var content = message.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            if (message.Embeds.Count > 0)
            {
                content = $"Embed: {message.Embeds.First().Title}: {message.Embeds.First().Description}";
            }
            else if (message.Attachments.Count > 0)
            {
                content = $"Attachment: {message.Attachments.First().Filename} {ByteSize.FromBytes(message.Attachments.First().Size)}";
            }
        }

        return new DeletedMessageInformation(message.Id, message.CreatedAt, message.GetJumpUrl(), message.Author.GetDisplayName(), content);
    }
}
