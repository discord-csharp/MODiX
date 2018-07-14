using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models.Core
{
    public class GuildConfig
    {
        [Key, Required]
        public long GuildConfigId { get; set; }

        public long GuildId { get; set; }
        public long AdminRoleId { get; set; }
        public long ModeratorRoleId { get; set; }
    }
}