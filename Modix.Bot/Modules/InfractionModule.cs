using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Modix.Utilities;

namespace Modix.Modules
{
    [Group("infraction"), Alias("infractions")]
    [Summary("Provides commands for working with infractions.")]
    public class InfractionModule : ModuleBase
    {
        public InfractionModule(IModerationService moderationService, IUserService userService)
        {
            ModerationService = moderationService;
            UserService = userService;
        }

        [Command("search")]
        [Summary("Display all infractions for a user, that haven't been deleted.")]
        public async Task Search(
            [Summary("The user whose infractions are to be displayed.")]
                IGuildUser subject)
        {
            await Search(subject.Id);
        }

        [Command("search")]
        [Summary("Display all infractions for a user, that haven't been deleted.")]
        [Priority(10)]
        public async Task Search(
            [Summary("The user whose infractions are to be displayed.")]
            ulong subjectId)
        {
            var requestor = Context.User.Mention;
            var subject = await UserService.GetGuildUserSummaryAsync(Context.Guild.Id, subjectId);

            var infractions = await ModerationService.SearchInfractionsAsync(
                new InfractionSearchCriteria
                {
                    GuildId = Context.Guild.Id,
                    SubjectId = subjectId,
                    IsDeleted = false
                },
                new[]
                {
                    new SortingCriteria { PropertyName = "CreateAction.Created", Direction = SortDirection.Descending }
                });

            if (infractions.Count == 0)
            {
                await ReplyAsync(Format.Code("No infractions"));
                return;
            }

            var infractionQuery = infractions.Select(infraction => new
            {
                Id = infraction.Id,
                Created = infraction.CreateAction.Created.ToUniversalTime().ToString("yyyy MMM dd"),
                Type = infraction.Type.ToString(),
                Subject = infraction.Subject.Username,
                Creator = infraction.CreateAction.CreatedBy.DisplayName,
                Reason = infraction.Reason,
                Rescinded = infraction.RescindAction != null
            }).OrderBy(s => s.Type);

            var counts = await ModerationService.GetInfractionCountsForUserAsync(subjectId);

            var builder = new EmbedBuilder()
                .WithTitle($"Infractions for user: {subject.Username}#{subject.Discriminator}")
                .WithDescription(FormatUtilities.FormatInfractionCounts(counts))
                .WithUrl($"https://mod.gg/infractions/?subject={subject.UserId}")
                .WithColor(new Color(0xA3BF0B))
                .WithTimestamp(DateTimeOffset.UtcNow);

            foreach (var infraction in infractionQuery)
            {
                builder.AddField(
                    $"#{infraction.Id} - {infraction.Type} - Created: {infraction.Created}{(infraction.Rescinded ? " - [RESCINDED]" : "")}",
                    $"[Reason: {infraction.Reason}](https://mod.gg/infractions/?id={infraction.Id})"
                );
            }

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                    $"Requested by {requestor}",
                    embed: embed)
                .ConfigureAwait(false);
        }

        [Command("delete")]
        [Summary("Marks an infraction as deleted, so it no longer appears within infraction search results")]
        public Task Delete(
            [Summary("The ID value of the infraction to be deleted.")]
                long infractionId)
            => ModerationService.DeleteInfractionAsync(infractionId);

        [Group("modify")]
        [Name("Infraction Modification")]
        [Summary("Provides commands for modifying infractions.")]
        public class ModifyModule : ModuleBase
        {
            public ModifyModule(IModerationService moderationService)
            {
                ModerationService = moderationService;
            }

            [Command("note")]
            [Summary("Updates an infraction to be a note.")]
            public async Task ModifyNote(
                [Summary("The ID value of the infraction to be modified.")]
                    long infractionId,
                [Summary("The reason for the note.")]
                [Remainder]
                    string reason = null)
            {
                await ModerationService.ModifyInfractionAsync(infractionId, InfractionType.Notice, reason: reason);

                await this.AddConfirmation();
            }

            [Command("warn")]
            [Summary("Updates an infraction to be a warning.")]
            public async Task ModifyWarn(
                [Summary("The ID value of the infraction to be modified.")]
                    long infractionId,
                [Summary("The reason for the warning.")]
                [Remainder]
                    string reason = null)
            {
                await ModerationService.ModifyInfractionAsync(infractionId, InfractionType.Warning, reason: reason);

                await this.AddConfirmation();
            }

            [Command("mute")]
            [Summary("Updates an infraction to be a mute.")]
            public async Task ModifyMute(
                [Summary("The ID value of the infraction to be modified.")]
                    long infractionId,
                [Summary("The reason for the mute.")]
                [Remainder]
                    string reason = null)
            {
                await ModerationService.ModifyInfractionAsync(infractionId, InfractionType.Mute, reason: reason);

                await this.AddConfirmation();
            }

            [Command("tempmute")]
            [Summary("Updates an infraction to be a temporary mute.")]
            public async Task ModifyTempMute(
                [Summary("The ID value of the infraction to be modified.")]
                    long infractionId,
                [Summary("The duration of the mute.")]
                    string durationString,
                [Summary("The reason for the mute.")]
                [Remainder]
                    string reason = null)
            {
                var duration = TimeSpanTypeReader.Read(durationString)
                    ?? throw new ArgumentException("Invalid timespan format.");

                await ModerationService.ModifyInfractionAsync(infractionId, InfractionType.Mute, duration, reason: reason);

                await this.AddConfirmation();
            }

            [Command("ban")]
            [Alias("forceban")]
            [Summary("Updates an infraction to be a ban.")]
            public async Task ModifyBan(
                [Summary("The ID value of the infraction to be modified.")]
                    long infractionId,
                [Summary("The reason for the ban.")]
                [Remainder]
                    string reason = null)
            {
                await ModerationService.ModifyInfractionAsync(infractionId, InfractionType.Ban, reason: reason);

                await this.AddConfirmation();
            }

            internal protected IModerationService ModerationService { get; }
        }

        internal protected IModerationService ModerationService { get; }
        public IUserService UserService { get; }
    }
}
