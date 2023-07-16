using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Modix.Data.Models.Core;
using Modix.Models;
using Modix.Services.Core;

namespace Modix.Controllers
{
    [Route("~/api/config/channels")]
    public class ChannelController : ModixController
    {
        private IDesignatedChannelService ChannelService { get; }

        public ChannelController(DiscordSocketClient client, IAuthorizationService modixAuth, IDesignatedChannelService channelService) : base(client, modixAuth)
        {
            ChannelService = channelService;
        }

        [HttpGet("types")]
        public IActionResult ChannelDesignationTypes()
        {
            return Ok(Enum.GetNames<DesignatedChannelType>());
        }

        [HttpGet]
        public async Task<IActionResult> ChannelDesignationsAsync()
        {
            var designatedChannels = await ChannelService.GetDesignatedChannelsAsync(ModixAuth.CurrentGuildId.Value);

            var mapped = designatedChannels.Select(d => new DesignatedChannelApiData
            {
                Id = d.Id,
                ChannelId = d.Channel.Id,
                ChannelDesignation = d.Type,
                Name = UserGuild?.GetChannel(d.Channel.Id)?.Name ?? d.Channel.Id.ToString()
            });

            return Ok(mapped);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDesignationAsync(long id)
        {
            await ChannelService.RemoveDesignatedChannelByIdAsync(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> CreateDesignationAsync([FromBody] DesignatedChannelCreationData creationData)
        {
            var foundChannel = DiscordSocketClient
                ?.GetGuild(ModixAuth.CurrentGuildId.Value)
                ?.GetChannel(creationData.ChannelId);

            if (foundChannel == null || foundChannel is not ISocketMessageChannel messageChannel)
            {
                return BadRequest($"A message channel was not found with id {creationData.ChannelId} in guild with id {ModixAuth.CurrentGuildId}");
            }

            foreach (var designation in creationData.ChannelDesignations)
            {
                await ChannelService.AddDesignatedChannelAsync(foundChannel.Guild, messageChannel, designation);
            }

            return Ok();
        }
    }
}
