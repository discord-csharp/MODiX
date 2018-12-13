using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Bot.Extensions;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;
using Modix.Services.Core;
using Modix.Services.Utilities;

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
        [Priority(10)]
        public async Task Search(
            [Summary("The user whose infractions are to be displayed.")]
            DiscordUserEntity subjectEntity)
        {
            var requestor = Context.User.Mention;
            var subject = await UserService.GetGuildUserSummaryAsync(Context.Guild.Id, subjectEntity.Id);

            var infractions = await ModerationService.SearchInfractionsAsync(
                new InfractionSearchCriteria
                {
                    GuildId = Context.Guild.Id,
                    SubjectId = subjectEntity.Id,
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

            var counts = await ModerationService.GetInfractionCountsForUserAsync(subjectEntity.Id);

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
        public async Task Delete(
            [Summary("The ID value of the infraction to be deleted.")]
                long infractionId)
        {
            await ModerationService.DeleteInfractionAsync(infractionId);
            await Context.AddConfirmation();
        }

        internal protected IModerationService ModerationService { get; }
        public IUserService UserService { get; }
    }
}
