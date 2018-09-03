using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Core
{
    public class MessageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public ulong MessageId { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        [Required]
        public string OriginalMessageHash { get; set; }
    }
}
