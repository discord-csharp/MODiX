using System.Collections.Generic;
using System.Linq;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Modix.RemoraShim.Errors
{
    internal record ChannelConfigurationError(string OperationName, IEnumerable<IChannel> Channels)
        : ResultError($"Operation {OperationName} failed for the following channels:\n{string.Join('\n', Channels.Select(x => $"{x.Name} ({x.ID})"))}");
}
