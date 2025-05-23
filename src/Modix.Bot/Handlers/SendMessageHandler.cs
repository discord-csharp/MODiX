using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Services;

namespace Modix.Bot.Handlers;

public class SendMessageHandler(DiscordSocketClient discordSocketClient) : INotificationHandler<SendMessageInDiscordRequest>
{
    public async Task Handle(SendMessageInDiscordRequest request, CancellationToken cancellationToken)
    {
        var channel = await discordSocketClient.GetChannelAsync(request.ChannelId);

        if (channel is ITextChannel textChannel)
        {
            await textChannel.SendMessageAsync(request.Message, false, request.Embed, allowedMentions: AllowedMentions.None);
        }
    }
}
