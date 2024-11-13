using System;
using System.Threading.Tasks;
using Discord;
using Modix.Common.Messaging;

namespace Modix.Services.AutoRemoveMessage;

public class AutoRemoveMessageService(IMessageDispatcher messageDispatcher)
{
    private const string FOOTER_MESSAGE = "React with ❌ to remove this embed.";

    public Task RegisterRemovableMessageAsync(IUser user, EmbedBuilder embed,
        Func<EmbedBuilder, Task<IUserMessage>> callback)
    {
        return RegisterRemovableMessageAsync([user], embed, callback);
    }

    public async Task RegisterRemovableMessageAsync(IUser[] users, EmbedBuilder embed,
        Func<EmbedBuilder, Task<IUserMessage>> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        if (embed.Footer?.Text == null)
        {
            embed.WithFooter(FOOTER_MESSAGE);
        }
        else if (!embed.Footer.Text.Contains(FOOTER_MESSAGE))
        {
            embed.Footer.Text += $" | {FOOTER_MESSAGE}";
        }

        var msg = await callback.Invoke(embed);
        messageDispatcher.Dispatch(new RemovableMessageSentNotification(msg, users));
    }

    public void UnregisterRemovableMessage(IMessage message)
        => messageDispatcher.Dispatch(new RemovableMessageRemovedNotification(message));
}
