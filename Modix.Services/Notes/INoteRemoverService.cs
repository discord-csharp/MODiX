namespace Modix.Services.Notes
{
    using System.Threading.Tasks;
    using Data.Models.Moderation;

    public interface INoteRemoverService
    {
        Task<bool> Remove(NoteEntity note);
    }
}