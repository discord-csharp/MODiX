using System.Threading.Tasks;
using Discord;
using MediatR;

namespace Modix.Services;

public class DiscordRelayService(IMediator mediator)
{
    public async Task<bool> SendMessageToChannel(ulong channelId, string message, Embed embed = null)
    {
        return await mediator.Send(new SendMessageInDiscordRequest(channelId, message, embed));
    }
}

public class SendMessageInDiscordRequest(ulong channelId, string message, Embed embed) : IRequest<bool>
{
    public ulong ChannelId { get; } = channelId;
    public string Message { get; } = message;
    public Embed Embed { get; } = embed;
}
