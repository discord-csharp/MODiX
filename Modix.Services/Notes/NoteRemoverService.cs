namespace Modix.Services.Notes
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using Data.Models.Moderation;
    using Serilog;

    public class NoteRemoverService : INoteRemoverService
    {
        private readonly ModixContext _modixContext;

        public NoteRemoverService(ModixContext modixContext)
        {
            _modixContext = modixContext;
        }

        public async Task<bool> Remove(NoteEntity note)
        {
            try
            {
                _modixContext.Notes.Remove(note);
                var rowsChanged = await _modixContext.SaveChangesAsync();

                return rowsChanged > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"RemoveNoteService RemoveNote has exceptioned with the following noteId: {note}");
                return false;
            }
        }
    }
}