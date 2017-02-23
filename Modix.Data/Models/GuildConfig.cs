
using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class GuildConfig
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public long AdminRoleId { get; set; }
        public long ModeratorRoleId { get; set; }
    }
}
