using System;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Mvc;

using Modix.Data.Models.Tags;
using Modix.Services.Core;
using Modix.Services.Tags;

namespace Modix.Controllers
{
    [ApiController]
    [Route("~/api/tags")]
    public class TagController : ModixController
    {
        private readonly ITagService _tagService;

        public TagController(DiscordSocketClient client, IAuthorizationService modixAuth, ITagService tagService) : base(client, modixAuth)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTagsAsync()
        {
            var summaries = await _tagService.GetSummariesAsync(new TagSearchCriteria
            {
                GuildId = UserGuild.Id,
            });

            var data = summaries.Select(x => new Models.Tags.TagData()
            {
                Content = x.Content,
                Created = x.CreateAction.Created,
                IsOwnedByRole = !(x.OwnerRole is null),
                Name = x.Name,
                OwnerUser = x.OwnerUser,
                OwnerRole = x.OwnerRole,
                Uses = x.Uses,
                TagSummary = x,
            })
            .ToArray();

            foreach (var tag in data)
            {
                // TODO Revisit this functionality
                tag.CanMaintain = false;
            }

            return Ok(data);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> CreateTagAsync(string name, Models.Tags.TagCreationData data)
        {
            try
            {
                await _tagService.CreateTagAsync(ModixUser.SelectedGuild, ModixUser.UserId, name, data.Content);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPatch("{name}")]
        public async Task<IActionResult> UpdateTagAsync(string name, Models.Tags.TagMutationData data)
        {
            try
            {
                await _tagService.ModifyTagAsync(ModixUser.SelectedGuild, ModixUser.UserId, name, data.Content);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteTagAsync(string name)
        {
            try
            {
                await _tagService.DeleteTagAsync(ModixUser.SelectedGuild, ModixUser.UserId, name);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
