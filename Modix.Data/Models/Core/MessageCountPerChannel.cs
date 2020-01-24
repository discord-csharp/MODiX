namespace Modix.Data.Models.Core
{
    public class MessageCountPerChannel
    {
        public ulong ChannelId { get; }
        public string ChannelName { get; }

        public MessageCountPerChannel(ulong channelId, string channelName)
        {
            ChannelId = channelId;
            ChannelName = channelName;
        }
    }
}
