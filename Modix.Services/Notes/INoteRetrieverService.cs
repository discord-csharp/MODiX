namespace Modix.Services.Notes
{
    using System.Collections.Generic;
    using Data.Models.Moderation;

    public interface INoteRetrieverService
    {
        List<NoteEntity> FindNotesForUser(ulong userId);
        NoteEntity FindNoteById(int noteId);
    }
}