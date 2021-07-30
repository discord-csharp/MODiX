using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Errors
{
    internal record NonThreadContextError(Snowflake ChannelId)
        : ResultError($"Command was executed outside of a thread. (ChannelId {ChannelId})");
}
