using System.Threading.Tasks;
using Discord;
using MediatR;

namespace Modix.Services;

public class DiscordRelayService(IMediator mediator)
{
    public async Task SendMessageToChannel(ulong channelId, string message, Embed embed = null)
    {
        await mediator.Publish(new SendMessageInDiscordRequest(channelId, message, embed));
    }
}

public class SendMessageInDiscordRequest(ulong channelId, string message, Embed embed) : INotification
{
    public ulong ChannelId { get; } = channelId;
    public string Message { get; } = message;
    public Embed Embed { get; } = embed;
}
