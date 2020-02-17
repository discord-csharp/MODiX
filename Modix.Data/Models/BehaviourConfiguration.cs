using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models
{
    public class BehaviourConfiguration
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public BehaviourCategory Category { get; set; }

        [Required]
        public string Key { get; set; } = null!;

        [Required]
        public string Value { get; set; } = null!;
    }

    public class BehaviourConfigurationConfigurator
        : IEntityTypeConfiguration<BehaviourConfiguration>
    {
        public void Configure(
            EntityTypeBuilder<BehaviourConfiguration> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Category)
                .HasConversion<string>();
        }
    }
}
