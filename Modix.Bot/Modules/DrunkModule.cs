using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Bot.Extensions;
using Modix.Data.Models.Moderation;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Drunk")]
    [Summary("Tools for evicting drunk users")]
    [HelpTags("sober", "drunk")]
    public class DrunkModule : ModuleBase
    {
        public DrunkModule(
            IModerationService moderationService,
            IUserService userService)
        {
            ModerationService = moderationService;
            UserService = userService;
        }

        private static readonly Dictionary<ulong, string> KnownAlcoholics = new Dictionary<ulong, string>
        {
            { 385135931922841600, "John" },
            { 335783158223994890, "Perksey" },
            { 104975006542372864, "Inzanit" }
        };

        [Command("drunk")]
        [Summary("Temporarily mute a drunk user")]
        public async Task DrunkAsync([Summary("The user to be muted")] DiscordUserOrMessageAuthorEntity subject)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            if (KnownAlcoholics.TryGetValue(subject.UserId, out string name))
            {
                await ReplyAsync($"{name} has got drunk again, what a suprise. You have a problem");
            }

            var dm = await (await Context.Guild.GetUserAsync(subject.UserId)).GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync($"You have been muted by {Context.User.Discriminator} because you are {Format.Underline("drunk")}!! \nMake sure to drink some water");

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Mute, subject.UserId, "Drunk", TimeSpan.FromHours(6));
        }

        [Command("sober")]
        [Summary("Rescind a drunk mute")]
        public async Task SoberAsync([Summary("The user to be unmuted")] DiscordUserOrMessageAuthorEntity subject)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            if (KnownAlcoholics.TryGetValue(subject.UserId, out string name))
            {
                await ReplyAsync($"{name} is sober? That is a first");
            }

            var dm = await (await Context.Guild.GetUserAsync(subject.UserId)).GetOrCreateDMChannelAsync();
            await dm.SendMessageAsync("Alright, you've been unmuted. Hope your hangover is awful");

            await ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.UserId);
        }

        private async ValueTask<bool> GetConfirmationIfRequiredAsync(DiscordUserOrMessageAuthorEntity userOrAuthor)
        {
            if (userOrAuthor.MessageId is null)
            {
                return true;
            }

            var author = await UserService.GetUserAsync(userOrAuthor.UserId);

            Debug.Assert(author is { }); // author should be nonnull, because we have a message written by someone with that ID

            return await Context.GetUserConfirmationAsync(
                "Detected a message ID instead of a user ID. Do you want to perform this action on "
                + $"{Format.Bold(author.GetFullUsername())} ({userOrAuthor.UserId}), the message's author?");
        }

        internal protected IModerationService ModerationService { get; }

        internal protected IUserService UserService { get; }
    }
}
