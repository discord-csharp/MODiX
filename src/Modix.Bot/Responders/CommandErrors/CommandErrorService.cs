using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace Modix.Bot.Responders.CommandErrors;

public class CommandErrorService(IMemoryCache memoryCache)
{
    private const string AssociatedErrorsKey = nameof(CommandErrorService) + ".AssociatedErrors";
    private const string ErrorRepliesKey = nameof(CommandErrorService) + ".ErrorReplies";

    public const string EMOJI = "⚠";

    private ConcurrentDictionary<ulong, string> AssociatedErrors =>
        memoryCache.GetOrCreate(AssociatedErrorsKey, _ => new ConcurrentDictionary<ulong, string>());

    private ConcurrentDictionary<ulong, ulong> ErrorReplies =>
        memoryCache.GetOrCreate(ErrorRepliesKey, _ => new ConcurrentDictionary<ulong, ulong>());

    public async Task SignalError(IMessage message, string error)
    {
        if (AssociatedErrors.TryAdd(message.Id, error))
        {
            await message.AddReactionAsync(new Emoji(EMOJI));
        }
    }

    public bool ContainsErrorReply(ulong cachedMessageId)
    {
        return ErrorReplies.ContainsKey(cachedMessageId);
    }

    public bool TryGetAssociatedError(ulong cachedMessageId, out string output)
    {
        return AssociatedErrors.TryGetValue(cachedMessageId, out output);
    }

    public bool TryAddErrorReply(ulong cachedMessageId, ulong messageId)
    {
        return ErrorReplies.TryAdd(cachedMessageId, messageId);
    }

    public bool TryGetErrorReply(ulong cachedMessageId, out ulong output)
    {
        return ErrorReplies.TryGetValue(cachedMessageId, out output);
    }

    public bool TryRemoveAssociatedError(ulong cachedMessageId)
    {
        return AssociatedErrors.TryRemove(cachedMessageId, out _);
    }

    public bool TryRemoveErrorReply(ulong cachedMessageId)
    {
        return ErrorReplies.TryRemove(cachedMessageId, out _);
    }
}
