using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class BehaviourConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public BehaviourCategory Category { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }

    public enum BehaviourCategory
    {
        InvitePurging = 0,
    }
}
