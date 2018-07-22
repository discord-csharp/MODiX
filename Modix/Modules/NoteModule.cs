using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.CommandHelp;
using Modix.Services.Moderation;
using Serilog;
using Tababular;

namespace Modix.Modules
{
    [Group("note"), Name("Add/Remove/Search Admin notes"), HiddenFromHelp]
    public class NoteModule : ModuleBase
    {
        private readonly IModerationService _moderationService;

        public NoteModule(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [Command("add"), Summary("Adds a note to a user")]
        public async Task CreateNote(IGuildUser user, [Remainder] string message)
        {
            var guildUser = Context.User as SocketGuildUser;

            // TODO: We shouldn't need to do this once claims are working
            if (!IsStaff(guildUser) && !IsOperator(guildUser))
            {
                await ReplyAsync($"I'm sorry, @{guildUser.Nickname}, I'm afraid I can't do that.");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    Log.Warning($"User: {Context.Message.Author.Username} has tried to create a note without a message");
                    await ReplyAsync("Please ensure that you have included a message with your note");
                }

                await _moderationService.CreateInfractionAsync(InfractionType.Notice, guildUser.Id, message, null);

                await ReplyAsync($"Note has been created for user: {guildUser.Username}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule CreateNote failed with the following user: {guildUser.Username} and message: {message}");
                await ReplyAsync("Uh oh. Looks like you are using the command wrong or an error has occured.");
            }
        }

        [Command("remove"), Summary("Removes a note from a user")]
        public async Task RemoveNote(long noteId)
        {
            var user = Context.User as SocketGuildUser;

            // TODO: We shouldn't need to do this once claims are working
            if (!IsStaff(user) && !IsOperator(user))
            {
                await ReplyAsync($"I'm sorry, @{user.Nickname}, I'm afraid I can't do that.");
                return;
            }

            try
            {
                await _moderationService.RescindInfractionAsync(noteId);

                await ReplyAsync($"Note {noteId} has been removed");
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule RemoveNote failed with the following noteId: {noteId}");
                await ReplyAsync($"Failed to remove the note: {noteId}");
            }
        }

        [Command("search"), Summary("Search notes for a user")]
        public async Task SearchNotesByUserId(ulong userId)
        {
            var user = Context.User as SocketGuildUser;

            // TODO: We shouldn't need to do this once claims are working
            if (!IsStaff(user) && !IsOperator(user))
            {
                await ReplyAsync($"I'm sorry, @{user.Nickname}, I'm afraid I can't do that.");
                return;
            }

            try
            {
                var notes = await _moderationService.SearchInfractionsAsync(
                    new InfractionSearchCriteria
                    {
                        SubjectId = userId,
                        Types = new[] { InfractionType.Notice }
                    },
                    new[]
                    {
                        new SortingCriteria { PropertyName = "CreateAction.Created", Direction = SortDirection.Descending }
                    });

                var sb = new StringBuilder();
                var hints = new Hints { MaxTableWidth = 100 };
                var formatter = new TableFormatter(hints);

                var formattedNotes = notes.Select(note => new
                {
                    Id = note.Id,
                    User = note.Subject.Username,
                    RecordedBy = note.CreateAction.CreatedBy,
                    Message = note.Reason,
                    Date = note.CreateAction.Created.ToString("yyyy-MM-ddTHH:mm:ss")
                }).ToList();

                var text = formatter.FormatObjects(formattedNotes);

                sb.Append(Format.Code(text));

                if (sb.ToString().Length <= 2000)
                {
                    await ReplyAsync(sb.ToString());
                    return;
                }

                var formattedNoteIds = notes.Select(note => new
                {
                    NoteId = note.Id
                });

                var noteIds = formatter.FormatObjects(formattedNoteIds);

                sb.Clear();
                sb.AppendLine("Notes exceed the character limit. Search for an Id below to retrieve note details");
                sb.Append(Format.Code(noteIds));

                await ReplyAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Log.Error(e, $"NoteModule SearchNotesByUserId failed with the following userId: {userId}");
                await ReplyAsync("Error occurred and search could not be complete");
            }
        }

        [Command("search"), Summary("Search notes for a username")]
        public async Task SearchNotesByUserName(IGuildUser user)
            => await SearchNotesByUserId(user.Id);

        private static bool IsOperator(SocketGuildUser user)
        {
            return user != null && user.Roles.Any(x => string.Equals("Operator", x.Name, StringComparison.Ordinal));
        }

        private static bool IsStaff(SocketGuildUser user)
        {
            return user != null && user.Roles.Any(
                x => string.Equals("Staff", x.Name, StringComparison.Ordinal) ||
                     string.Equals("Moderator", x.Name, StringComparison.Ordinal) ||
                     string.Equals("Administrator", x.Name, StringComparison.Ordinal));
        }
    }
}