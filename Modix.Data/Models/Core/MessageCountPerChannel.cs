namespace Modix.Data.Models.Core
{
    public class MessageCountPerChannel
    {
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; } = null!;
        public int MessageCount { get; set; }
    }
}
