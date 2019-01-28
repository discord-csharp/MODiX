using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("Moderation")]
    [Summary("Guild moderation commands")]
    public class ModerationModule : ModuleBase
    {
        public ModerationModule(IModerationService moderationService)
        {
            ModerationService = moderationService;
        }

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public async Task Note(
            [Summary("The user to which the note is being applied.")]
                DiscordUserEntity subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Notice, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public async Task Warn(
            [Summary("The user to which the warning is being issued.")]
                DiscordUserEntity subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Warning, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("mute")]
        [Summary("Mute a user.")]
        public async Task Mute(
            [Summary("The user to be muted.")]
                DiscordUserEntity subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("tempmute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public async Task TempMute(
            [Summary("The user to be muted.")]
                DiscordUserEntity subject,
            [Summary("The duration of the mute.")]
                TimeSpan duration,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, duration);
            await Context.AddConfirmation();
        }

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public async Task UnMute(
            [Summary("The user to be un-muted.")]
                DiscordUserEntity subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.Id);
        }

        [Command("ban")]
        [Alias("forceban")]
        [Summary("Ban a user from the current guild.")]
        public async Task Ban(
            [Summary("The user to be banned.")]
                DiscordUserEntity subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Ban, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public async Task UnBan(
            [Summary("The user to be un-banned.")]
                DiscordUserEntity subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Ban, subject.Id);
            await Context.AddConfirmation();
        }

        internal protected IModerationService ModerationService { get; }
    }
}
