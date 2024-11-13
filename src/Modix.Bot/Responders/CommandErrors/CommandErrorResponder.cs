using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Bot.Notifications;

namespace Modix.Bot.Responders.CommandErrors;

public class CommandErrorResponder(DiscordSocketClient discordSocketClient, CommandErrorService service) :
    INotificationHandler<ReactionAddedNotificationV3>,
    INotificationHandler<ReactionRemovedNotificationV3>
{
    private readonly IEmote _emote = new Emoji(CommandErrorService.EMOJI);

    public Task Handle(ReactionAddedNotificationV3 notification, CancellationToken cancellationToken)
        => ReactionAddedAsync(notification.Message, notification.Channel, notification.Reaction);

    public async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        //Don't trigger if the emoji is wrong, if the user is a bot, or if we've
        //made an error message reply already

        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
        {
            return;
        }

        if (reaction.Emote.Name != CommandErrorService.EMOJI || service.ContainsErrorReply(cachedMessage.Id))
        {
            return;
        }

        // If the message that was reacted to has an associated error, reply in the same channel
        // with the error message then add that to the replies collection
        if (service.TryGetAssociatedError(cachedMessage.Id, out var value))
        {
            var channel = await cachedChannel.GetOrDownloadAsync();
            var msg = await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = "https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/26a0.png",
                    Name = "That command had an error"
                },
                Description = value,
                Footer = new EmbedFooterBuilder { Text = "Remove your reaction to delete this message" }
            }.Build());

            if (service.TryAddErrorReply(cachedMessage.Id, msg.Id) == false)
            {
                await msg.DeleteAsync();
            }
        }
    }

    public Task Handle(ReactionRemovedNotificationV3 notification, CancellationToken cancellationToken)
        => ReactionRemovedAsync(notification.Message, notification.Channel, notification.Reaction);

    public async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        //Bugfix for NRE?
        if (reaction is null || reaction.User.Value is null)
        {
            return;
        }

        //Don't trigger if the emoji is wrong, or if the user is bot
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
        {
            return;
        }

        if (reaction.Emote.Name != CommandErrorService.EMOJI)
        {
            return;
        }

        //If there's an error reply when the reaction is removed, delete that reply,
        //remove the cached error, remove it from the cached replies, and remove
        //the reactions from the original message
        if (service.TryGetErrorReply(cachedMessage.Id, out var botReplyId) == false) { return; }

        var channel = await cachedChannel.GetOrDownloadAsync();
        await channel.DeleteMessageAsync(botReplyId);

        if (service.TryRemoveAssociatedError(cachedMessage.Id) && service.TryRemoveErrorReply(cachedMessage.Id))
        {
            var originalMessage = await cachedMessage.GetOrDownloadAsync();

            //If we know what user added the reaction, remove their and our reaction
            //Otherwise just remove ours

            if (reaction.User.IsSpecified)
            {
                await originalMessage.RemoveReactionAsync(_emote, reaction.User.Value);
            }

            await originalMessage.RemoveReactionAsync(_emote, discordSocketClient.CurrentUser);
        }
    }
}
