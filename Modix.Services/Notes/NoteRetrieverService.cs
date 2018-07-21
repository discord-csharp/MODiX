namespace Modix.Services.Notes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Data.Models.Moderation;
    using Serilog;

    public class NoteRetrieverService : INoteRetrieverService
    {
        private readonly ModixContext _modixContext;

        public NoteRetrieverService(ModixContext modixContext)
        {
            _modixContext = modixContext;
        }

        public List<NoteEntity> FindNotesForUser(ulong userId)
        {
            try
            {
                var notes = _modixContext.Notes.Where(n => n.UserId == userId).ToList();
                return notes;
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteRetrieverService FindNoteForUser failed with the following userId: {userId}");
                return null;
            }
        }

        public NoteEntity FindNoteById(int noteId)
        {
            try
            {
                var note = _modixContext.Notes.FirstOrDefault(n => n.Id == noteId);
                return note;
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteRetrieverService FindNoteById has failed with the following noteId: {noteId}");
                return null;
            }
        }
    }
}