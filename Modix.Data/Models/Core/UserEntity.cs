using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a user of the application, that has previously joined a Discord guild managed by MODiX.
    /// </summary>
    [Table("Users")]
    public class UserEntity
    {
        /// <summary>
        /// The unique identifier for this user within the Discord API.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        /// <summary>
        /// The "username" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// The "discriminator" value of this user, within the Discord API.
        /// </summary>
        [Required]
        public string Discriminator { get; set; } = null!;
    }

    public class UserEntityConfigurator
        : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(
            EntityTypeBuilder<UserEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Id)
                .HasConversion<long>();
        }
    }
}
