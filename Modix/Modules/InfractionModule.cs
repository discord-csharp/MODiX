using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;
using Serilog;
using Tababular;

namespace Modix.Modules
{
    public class InfractionModule : ModuleBase
    {
        private readonly IModerationService _moderationService;

        public InfractionModule(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }


        [Command("search"), Summary("Search infractions for a user")]
        public async Task SearchInfractionsByUserId(ulong userId)
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
                        SubjectId = userId
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
