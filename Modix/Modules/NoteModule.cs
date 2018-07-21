namespace Modix.Modules
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Serilog;
    using Services.CommandHelp;
    using Services.Notes;
    using Tababular;

    [Name("Add/Remove/Search Admin notes"), RequireUserPermission(GuildPermission.Administrator), HiddenFromHelp]
    public class NoteModule : ModuleBase
    {
        private readonly INoteCreatorService _noteCreatorService;
        private readonly INoteRemoverService _noteRemoverService;
        private readonly INoteRetrieverService _noteRetrieverService;

        public NoteModule(INoteCreatorService noteCreatorService, INoteRemoverService noteRemoverService, INoteRetrieverService noteRetrieverService)
        {
            _noteCreatorService = noteCreatorService;
            _noteRemoverService = noteRemoverService;
            _noteRetrieverService = noteRetrieverService;
        }


        [Command("note add"), Summary("Adds a note to a user")]
        public async Task CreateNote(IGuildUser user, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    Log.Warning($"User: {Context.Message.Author.Username} has tried to create a note without a message");
                    await ReplyAsync("Please ensure that you have included a message with your note");
                }

                var result = await _noteCreatorService.Create(user, message, Context.Message.Author.Username);

                if (!result)
                {
                    await ReplyAsync($"No note has been added for user {user.Username} due to an error. Please see the error logs");
                }

                await ReplyAsync($"Note has been created for user: {user.Username}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule CreateNote failed with the following user: {user.Username} and message: {message}");
                await ReplyAsync("Uh oh. Looks like you are using the command wrong or an error has occured.");
            }
        }

        [Command("note remove"), Summary("Removes a note from a user")]
        public async Task RemoveNote(int noteId)
        {
            try
            {
                var note = _noteRetrieverService.FindNoteById(noteId);

                if (note == null)
                {
                    Log.Warning($"User: {Context.Client.CurrentUser} has tried to remove a note that does not exist");
                    await ReplyAsync("Note does not exist for that Id");
                }

                var result = await _noteRemoverService.Remove(note);

                if (!result)
                {
                    await ReplyAsync($"Error occurred and note could not be removed");
                }

                await ReplyAsync($"Note {noteId} has been removed");
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule RemoveNote failed with the following noteId: {noteId}");
                await ReplyAsync($"Failed to remove the note: {noteId}");
            }
        }

        [Command("note search"), Summary("Search notes for a user")]
        public async Task SearchNotesByUserId(ulong userId)
        {
            try
            {
                var notes = _noteRetrieverService.FindNotesForUser(userId);

                if (notes == null)
                {
                    Log.Error("Notes has thrown an error");
                    await ReplyAsync($"No notes found for user {userId}");
                }


                if (notes.Count == 0)
                {
                    await ReplyAsync($"No notes found for user {userId}");
                    return;
                }

                var sb = new StringBuilder();
                var hints = new Hints { MaxTableWidth = 100 };
                var formatter = new TableFormatter(hints);
                sb.AppendLine("```");

                var formattedNotes = notes.Select(note => new
                {
                    Id = $"{note.Id}",
                    User = $"{note.UserName}",
                    RecordedBy = $"{note.RecordedBy}",
                    Message = $"{note.Message}",
                    Date = $"{note.RecordedTime}"
                }).ToList();

                var text = formatter.FormatObjects(formattedNotes);
                sb.Append(text);

                sb.AppendLine("```");

                if (sb.ToString().Length <= 2000)
                {
                    await ReplyAsync(sb.ToString());
                    return;
                }

                var formattedNoteIds = notes.Select(note => new
                {
                    NoteId = $"{note.Id}"
                });

                var noteIds = formatter.FormatObjects(formattedNoteIds);

                sb.Clear();
                sb.AppendLine("Notes exceed the character limit. Search for an Id below to retrieve note details");
                sb.AppendLine("```");
                sb.Append(noteIds);
                sb.AppendLine("```");

                await ReplyAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule SearchNotesByUserId failed with the following userId: {userId}");
                await ReplyAsync("Error occurred and search could not be complete");
            }
        }

        [Command("note search"), Summary("Search notes for a username")]
        public async Task SearchNotesByUserName(IGuildUser user)
            => await SearchNotesByUserId(user.Id);

        [Command("note search id"), Summary("Search notes for a user")]
        public async Task SearchNotesByNoteId(int noteId)
        {
            try
            {
                var note = _noteRetrieverService.FindNoteById(noteId);

                if (note == null)
                {
                    await ReplyAsync($"No note found for id: {noteId}");
                    return;
                }

                var sb = new StringBuilder();
                var hints = new Hints { MaxTableWidth = 100 };
                var formatter = new TableFormatter(hints);
                sb.AppendLine("```");

                var formattedNotes = new []
                {
                    new
                    {
                        Id = $"{note.Id}",
                        User = $"{note.UserName}",
                        RecordedBy = $"{note.RecordedBy}",
                        Message = $"{note.Message}",
                        Date = $"{note.RecordedTime}"
                    }
                };

                var text = formatter.FormatObjects(formattedNotes);
                sb.Append(text);

                sb.AppendLine("```");

                await ReplyAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule SearchNotesByNoteId failed with the following noteId: {noteId}");
                await ReplyAsync("Error occurred and search could not be complete");
            }
        }
    }
}