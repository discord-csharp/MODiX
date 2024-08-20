using System.Diagnostics;
using System.Security.Claims;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Services.Tags;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Tags;

namespace Modix.Web.Controllers;

[Route("~/api/tags")]
[ApiController]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly DiscordSocketClient _discordSocketClient;

    public TagsController(ITagService tagService, DiscordSocketClient discordSocketClient)
    {
        _tagService = tagService;
        _discordSocketClient = discordSocketClient;
    }

    [HttpGet]
    public async Task<IEnumerable<TagData>> GetTagsAsync()
    {
        // TODO: Move this to a base class like ModixController?

        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

        SocketGuild guildToSearch;
        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = _discordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = _discordSocketClient.Guilds.First();
        }

        var summaries = await _tagService.GetSummariesAsync(new TagSearchCriteria
        {
            GuildId = guildToSearch.Id,
        });

        return summaries.Select(CreateFromSummary);
    }

    [HttpPut]
    [Authorize(Roles = nameof(AuthorizationClaim.CreateTag))]
    public async Task<IActionResult> CreateTagAsync([FromBody] TagCreationData tagCreationData)
    {
        try
        {
            // TODO: Move this to a base class like ModixController?

            var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

            SocketGuild guildToSearch;
            if (!string.IsNullOrWhiteSpace(guildCookie))
            {
                var guildId = ulong.Parse(guildCookie);
                guildToSearch = _discordSocketClient.GetGuild(guildId);
            }
            else
            {
                guildToSearch = _discordSocketClient.Guilds.First();
            }

            var userId = ulong.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            await _tagService.CreateTagAsync(guildToSearch.Id, userId, tagCreationData.Name, tagCreationData.Content);
            var createdTag = await _tagService.GetTagAsync(guildToSearch.Id, tagCreationData.Name);

            var tagSummary = CreateFromSummary(createdTag);

            return Ok(tagSummary);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    private static TagData CreateFromSummary(TagSummary summary)
    {
        return new TagData(
                summary.Name,
                summary.CreateAction.Created,
                summary.OwnerRole is not null,
                summary.OwnerUser?.Id ?? summary.OwnerRole?.Id ?? throw new UnreachableException("No owner??"),
                summary.OwnerUser?.Username ?? summary.OwnerRole?.Name ?? throw new UnreachableException("No owner??"),
                summary.Content,
                summary.Uses,
                false);
    }
}
