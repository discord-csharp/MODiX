#nullable enable

using Modix.Common;

namespace Modix.Services
{
    public enum ServicesLogEventType
    {
        Roles           = ApplicationLogEventType.Services + 0x010000,
        MessageTracking = ApplicationLogEventType.Services + 0x020000,
        MessageLogging  = ApplicationLogEventType.Services + 0x030000,

        // The below value used to be used by the UserMetricsBehavior
        // (https://bit.ly/3CWEkip). This behavior has since been removed,
        // but leaving this here in case somebody wonders why we jumped
        // from 0x030000 to 0x050000.
        //
        // UserMetrics     = ApplicationLogEventType.Services + 0x040000,

        Moderation = ApplicationLogEventType.Services + 0x050000
    }
}
