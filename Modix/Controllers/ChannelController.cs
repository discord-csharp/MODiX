using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> ChannelDesignations()
        {
            var designatedChannels = await ChannelService.GetDesignatedChannelsAsync(ModixAuth.CurrentGuildId.Value);

            var mapped = designatedChannels.Select(d => new DesignatedChannelApiData
            {
                Id = d.Id,
                ChannelId = d.Channel.Id,
                ChannelDesignation = d.Type,
                Name = UserGuild?.GetChannel(d.Channel.Id).Name ?? d.Id.ToString()
            });

            return Ok(mapped);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDesignation(long id)
        {
            await ChannelService.RemoveDesignatedChannelByIdAsync(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> CreateDesignation([FromBody] DesignatedChannelCreationData creationData)
        {
            var foundChannel = DiscordSocketClient
                ?.GetGuild(ModixAuth.CurrentGuildId.Value)
                ?.GetChannel(creationData.ChannelId);

            if (foundChannel == null || !(foundChannel is ISocketMessageChannel messageChannel))
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
