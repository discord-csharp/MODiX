using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class GuildConfig
    {
        [Key, Required] public long GuildConfigID { get; set; }

        public long GuildId { get; set; }
        public long AdminRoleId { get; set; }
        public long ModeratorRoleId { get; set; }
    }
}