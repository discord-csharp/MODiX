using System.Linq;
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
                Created = infraction.CreateAction.Created.ToUniversalTime().ToString("yyyy MMM dd HH:mm"),
                Type = infraction.Type.ToString(),
                Subject = infraction.Subject.Username,
                Creator = infraction.CreateAction.CreatedBy.DisplayName,
                State = (infraction.RescindAction != null) ? "Rescinded"
                    : (infraction.Expires != null) ? "Will Expire"
                    : "Active",
                Reason = infraction.Reason
            }).OrderBy(s => s.Type);

            var builder = new EmbedBuilder()
                .WithTitle($"Infractions for user {subject.Nickname}")
                //.WithDescription("This user has a total of 5 mutes, 7 warnings, and 3 notices.")
                .WithUrl("https://discordapp.com")
                .WithColor(new Color(0xA3BF0B))
                //.WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(1537508496766))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("Infractions")
                        .WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/embed/avatars/0.png");


            foreach (var infraction in infractionQuery)
            {
                builder.AddField(infraction.Type, $"ID: {infraction.Id} - Reason: {infraction.Reason}");
            }

            var embed = builder.Build();
            
            await Context.Channel.SendMessageAsync(
                    "Requested by @username",
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
