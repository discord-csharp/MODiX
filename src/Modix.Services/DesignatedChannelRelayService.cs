using System.Threading.Tasks;
using MediatR;
using Modix.Data.Models.Core;

namespace Modix.Services;

public class DesignatedChannelRelayService(DesignatedChannelService designatedChannelService, IMediator mediator)
{
    public async Task RelayMessageToGuild(DesignatedChannelType type, ulong guildId, string message)
    {
        var channels = await designatedChannelService.GetDesignatedChannelIds(guildId, type);

        foreach (var channel in channels)
        {
            await mediator.Publish(new SendMessageInDiscordRequest(channel, message, null));
        }
    }
}
