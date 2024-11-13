using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Modix.Services.AutoRemoveMessage;

namespace Modix.Bot.Responders.AutoRemoveMessages;

public class AutoRemoveMessageService(IMemoryCache memoryCache)
{
    private static readonly MemoryCacheEntryOptions _messageCacheOptions =
        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));

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

        var message = await callback.Invoke(embed);

        memoryCache.Set(
            GetCacheKey(message.Id),
            new RemovableMessage
            {
                Message = message,
                Users = users
            },
            _messageCacheOptions);
    }

    public bool IsKnownRemovableMessage(ulong messageId, out RemovableMessage removableMessage)
    {
        var key = GetCacheKey(messageId);

        if (memoryCache.TryGetValue(key, out var cached))
        {
            removableMessage = (RemovableMessage)cached;
            return true;
        }

        removableMessage = default;
        return false;
    }

    public void UnregisterRemovableMessage(IMessage message)
    {
        var key = GetCacheKey(message.Id);

        if (!memoryCache.TryGetValue(key, out _))
        {
            return;
        }

        memoryCache.Remove(key);
    }


    private static object GetCacheKey(ulong messageId)
        => new
        {
            MessageId = messageId,
            Target = "RemovableMessage",
        };
}
