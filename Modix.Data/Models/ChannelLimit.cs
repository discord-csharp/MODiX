using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class ChannelLimit
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ChannelLimitID { get; set; }

        public long ChannelId { get; set; }
        public string ModuleName { get; set; }
        public virtual DiscordGuild Guild { get; set; }
    }
}