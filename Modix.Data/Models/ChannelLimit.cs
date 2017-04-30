namespace Modix.Data.Models
{
    public class ChannelLimit
    {
        public int Id { get; set; }
        public long ChannelId { get; set; }
        public string ModuleName { get; set; }
        public virtual DiscordGuild Guild { get; set; }
    }
}
