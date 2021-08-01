using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace Modix.RemoraShim.Services
{
    public interface IThreadService
    {
        Task<bool> IsThreadChannelAsync(Snowflake channelId, CancellationToken ct = default);
    }

    public class ThreadService : IThreadService
    {
        private readonly IMemoryCache _cache;
        private readonly IDiscordRestChannelAPI _channelApi;

        public ThreadService(IMemoryCache cache, IDiscordRestChannelAPI channelApi)
        {
            _cache = cache;
            _channelApi = channelApi;
        }

        public async Task<bool> IsThreadChannelAsync(Snowflake channelId, CancellationToken ct = default)
        {
            var knownChannelsKey = $"{nameof(ThreadService)}.{nameof(IsThreadChannelAsync)}:known-thread-channels";
            _cache.TryGetValue(knownChannelsKey, out ConcurrentDictionary<Snowflake, bool> channels);
            channels ??= new ConcurrentDictionary<Snowflake, bool>();

            if (channels.ContainsKey(channelId))
            {
                return channels[channelId];
            }
            else
            {
                var channel = await _channelApi.GetChannelAsync(channelId, ct);
                var isThread = channel.Entity!.ThreadMetadata.HasValue;
                channels.TryAdd(channelId, isThread);
                _cache.Set(knownChannelsKey, channels);
                return isThread;
            }
        }
    }

}
