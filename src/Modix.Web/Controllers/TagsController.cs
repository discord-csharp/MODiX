using System.Diagnostics;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models.Tags;
using Modix.Models.Core;
using Modix.Services.Tags;
using Modix.Web.Shared.Models.Tags;

namespace Modix.Web.Controllers;

[Route("~/api/tags")]
[ApiController]
[Authorize]
public class TagsController : ModixController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IEnumerable<TagData>> GetTagsAsync()
    {
        var summaries = await _tagService.GetSummariesAsync(new TagSearchCriteria
        {
            GuildId = UserGuild.Id,
        });

        return summaries.Select(CreateFromSummary);
    }

    [HttpPut]
    [Authorize(Roles = nameof(AuthorizationClaim.CreateTag))]
    public async Task<IActionResult> CreateTagAsync([FromBody] TagCreationData tagCreationData)
    {
        try
        {
            await _tagService.CreateTagAsync(UserGuild.Id, SocketUser.Id, tagCreationData.Name, tagCreationData.Content);
            var createdTag = await _tagService.GetTagAsync(UserGuild.Id, tagCreationData.Name);

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
