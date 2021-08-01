using System.Threading;
using System.Threading.Tasks;
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
        private readonly IDiscordRestChannelAPI _channelApi;

        public ThreadService(IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public async Task<bool> IsThreadChannelAsync(Snowflake channelId, CancellationToken ct = default)
        {
            var channel = await _channelApi.GetChannelAsync(channelId, ct);
            var isThread = channel.Entity!.ThreadMetadata.HasValue;
            return isThread;
        }
    }
}
