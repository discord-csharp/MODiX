#nullable enable

using Modix.Common;

namespace Modix.Services
{
    public enum ServicesLogEventType
    {
        Roles           = ApplicationLogEventType.Services + 0x010000,
        MessageTracking = ApplicationLogEventType.Services + 0x020000,
        MessageLogging  = ApplicationLogEventType.Services + 0x030000,
        UserMetrics     = ApplicationLogEventType.Services + 0x040000,
        Moderation      = ApplicationLogEventType.Services + 0x050000
    }
}
