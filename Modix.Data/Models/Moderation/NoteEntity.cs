namespace Modix.Data.Models.Moderation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NoteEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime RecordedTime { get; set; }
        public string RecordedBy { get; set; }
    }
}