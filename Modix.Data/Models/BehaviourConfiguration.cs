using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

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

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<BehaviourConfiguration>()
                .Property(x => x.Category)
                .HasConversion<string>();
        }
    }
}
