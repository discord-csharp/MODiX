namespace Modix.Services.Notes
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using Data.Models.Moderation;
    using Discord;
    using Serilog;

    public class NoteCreatorService : INoteCreatorService
    {
        private readonly ModixContext _modixContext;

        public NoteCreatorService(ModixContext modixContext)
        {
            _modixContext = modixContext;
        }

        public async Task<bool> Create(IGuildUser user, string message, string recordedBy)
        {
            try
            {
                var note = new NoteEntity
                {
                    UserName = user.Username,
                    UserId = user.Id,
                    Message = message,
                    RecordedTime = DateTime.UtcNow,
                    RecordedBy = recordedBy
                };

                await _modixContext.Notes.AddAsync(note);
                var rowsAdded = await _modixContext.SaveChangesAsync();

                return rowsAdded > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"AddNoteService Create has exceptioned with the following user: {user.Username} and message: {message}");
                return false;
            }
        }
    }
}