using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Tababular;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Group("infraction")]
    [Summary("Provides commands for working with infractions.")]
    public class InfractionModule : ModuleBase
    {
        public InfractionModule(IModerationService moderationService)
        {
            ModerationService = moderationService;
        }

        [Command("search")]
        [Summary("Display all infractions for a user, that haven't been deleted.")]
        public async Task Search(
            [Summary("The user whose infractions are to be displayed.")]
                IGuildUser subject)
        {
            var infractions = await ModerationService.SearchInfractionsAsync(
                new InfractionSearchCriteria
                {
                    GuildId = subject.GuildId,
                    SubjectId = subject.Id,
                    IsDeleted = false
                },
                new[]
                {
                    new SortingCriteria { PropertyName = "CreateAction.Created", Direction = SortDirection.Descending }
                });

            if(infractions.Count == 0)
            {
                await ReplyAsync(Format.Code("No infractions"));
                return;
            }

            var hints = new Hints { MaxTableWidth = 100 };
            var formatter = new TableFormatter(hints);

            var tableText = formatter.FormatObjects(infractions.Select(infraction => new
            {
                Id = infraction.Id,
                Created = infraction.CreateAction.Created.ToUniversalTime().ToString("yyyy MMM dd HH:mm"),
                Type = infraction.Type.ToString(),
                Subject = infraction.Subject.Username,
                Creator = infraction.CreateAction.CreatedBy.DisplayName,
                State = (infraction.RescindAction != null) ? "Rescinded"
                    : (infraction.Expires != null) ? "Will Expire"
                    : "Active",
                Reason = infraction.Reason
            }));

            var replyBuilder = new StringBuilder();
            foreach (var line in tableText.Split("\r\n"))
            {
                if((replyBuilder.Length + line.Length) > 1998)
                {
                    await ReplyAsync(Format.Code(replyBuilder.ToString()));
                    replyBuilder.Clear();
                }
                replyBuilder.AppendLine(line);
            }

            if(replyBuilder.Length > 0)
                await ReplyAsync(Format.Code(replyBuilder.ToString()));
        }
        
        [Command("search embed")]
        [Summary("Display all infractions for a user, that haven't been deleted.")]
        public async Task SearchEmbed(
            [Summary("The user whose infractions are to be displayed.")]
            IGuildUser subject)
        {
            var requestor = Context.User.Mention;
            
            var infractions = await ModerationService.SearchInfractionsAsync(
                new InfractionSearchCriteria
                {
                    GuildId = subject.GuildId,
                    SubjectId = subject.Id,
                    IsDeleted = false
                },
                new[]
                {
                    new SortingCriteria { PropertyName = "CreateAction.Created", Direction = SortDirection.Descending }
                });

            if(infractions.Count == 0)
            {
                await ReplyAsync(Format.Code("No infractions"));
                return;
            }

            var infractionQuery = infractions.Select(infraction => new
            {
                Id = infraction.Id,
                Created = infraction.CreateAction.Created.ToUniversalTime().ToString("MMM dd, yyyy HH:mm tt zz"),
                Type = infraction.Type.ToString(),
                Subject = infraction.Subject.Username,
                Creator = infraction.CreateAction.CreatedBy.DisplayName,
                State = (infraction.RescindAction != null) ? "Rescinded"
                    : (infraction.Expires != null) ? "Will Expire"
                    : "Active",
                Reason = infraction.Reason
            }).OrderBy(s => s.Type);

            var noticeCount = infractions.Count(x => x.Type == InfractionType.Notice);
            var warningCount = infractions.Count(x => x.Type == InfractionType.Warning);
            var muteCount = infractions.Count(x => x.Type == InfractionType.Mute);
            var banCount = infractions.Count(x => x.Type == InfractionType.Ban);

            var builder = new EmbedBuilder()
                .WithTitle($"Infractions for user: {subject.Username}#{subject.Discriminator} - {subject.Id}")
                .WithDescription(
                    $"This user has {noticeCount} notice(s), {warningCount} warning(s), {muteCount} mute(s), and {banCount} ban(s)")
                .WithUrl("https://mod.gg/infractions")
                .WithColor(new Color(0xA3BF0B))
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Infractions - https://mod.gg/infractions");
                });

            foreach (var infraction in infractionQuery)
            {
                builder.AddField($"{infraction.Type} - Created: {infraction.Created}", $"ID: {infraction.Id} - Reason: {infraction.Reason}");
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

        internal protected IModerationService ModerationService { get; }
    }
}
