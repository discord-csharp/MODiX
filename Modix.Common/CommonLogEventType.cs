namespace Modix.Common
{
    public enum CommonLogEventType
    {
        DependencyInjection = ApplicationLogEventType.Common + 0x010000,
        Hosting             = ApplicationLogEventType.Common + 0x020000,
        Messaging           = ApplicationLogEventType.Common + 0x030000
    }
}
