﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models;
using Modix.Data.Models.Core;
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
        public InfractionModule(IModerationService moderationService, IUserService userService, IOptions<ModixConfig> config)
        {
            ModerationService = moderationService;
            UserService = userService;
            Config = config.Value;
        }

        [Command]
        [Alias("search")]
        [Summary("Display all infractions for a user, that haven't been deleted.")]
        [Priority(10)]
        public async Task SearchAsync(
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
                    new SortingCriteria()
                    {
                        PropertyName = nameof(InfractionSummary.CreateAction.Created),
                        Direction = SortDirection.Descending,
                    },
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
                Creator = infraction.CreateAction.CreatedBy.GetFullUsername(),
                Reason = infraction.Reason,
                Rescinded = infraction.RescindAction != null
            }).OrderBy(s => s.Type);

            var counts = await ModerationService.GetInfractionCountsForUserAsync(subjectEntity.Id);

            // https://modix.gg/infractions?subject=12345
            var url = new UriBuilder(Config.WebsiteBaseUrl)
            {
                Path = "/infractions",
                Query = $"subject={subject.UserId}"
            }.RemoveDefaultPort().ToString();

            var builder = new EmbedBuilder()
                .WithTitle($"Infractions for user: {subject.GetFullUsername()}")
                .WithDescription(FormatUtilities.FormatInfractionCounts(counts))
                .WithUrl(url)
                .WithColor(new Color(0xA3BF0B));

            foreach (var infraction in infractionQuery)
            {
                // https://modix.gg/infractions?id=123
                var infractionUrl = new UriBuilder(Config.WebsiteBaseUrl)
                {
                    Path = "/infractions",
                    Query = $"id={infraction.Id}"
                }.ToString();
                builder.AddField(
                    $"#{infraction.Id} - {infraction.Type} - Created: {infraction.Created}{(infraction.Rescinded ? " - [RESCINDED]" : "")}",
                    Format.Url($"Reason: {infraction.Reason}", infractionUrl)
                );
            }

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                    $"Requested by {requestor}",
                    embed: embed)
                .ConfigureAwait(false);
        }

        [Command("delete")]
        [Summary("Marks an infraction as deleted, so it no longer appears within infraction search results.")]
        public async Task DeleteAsync(
            [Summary("The ID value of the infraction to be deleted.")]
                long infractionId)
        {
            await ModerationService.DeleteInfractionAsync(infractionId);
            await Context.AddConfirmation();
        }

        internal protected IModerationService ModerationService { get; }
        internal protected IUserService UserService { get; }
        internal protected ModixConfig Config { get; }
    }
}
