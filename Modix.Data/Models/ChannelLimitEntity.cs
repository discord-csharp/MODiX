using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class ChannelLimitEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ChannelLimitID { get; set; }

        public long ChannelId { get; set; }
        public string ModuleName { get; set; }
        public virtual DiscordGuildEntity Guild { get; set; }
    }
}