using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class ChannelLimit
    {
        [Key, Required] public long ChannelLimitID { get; set; }

        public long ChannelId { get; set; }
        public string ModuleName { get; set; }
        public virtual DiscordGuild Guild { get; set; }
    }
}