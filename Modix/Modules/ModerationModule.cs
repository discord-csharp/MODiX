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

        public async Task AddConfirmationEmoji()
        {
            await Context.Message.AddReactionAsync(new Emoji("🆗"));
        }

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public async Task Note(
            [Summary("The user to which the note is being applied.")]
                IGuildUser subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Notice, subject.Id, reason, null);
            await AddConfirmationEmoji();
        }

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public async Task Warn(
            [Summary("The user to which the warning is being issued.")]
                IGuildUser subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Warning, subject.Id, reason, null);
            await AddConfirmationEmoji();
        }

        [Command("mute")]
        [Summary("Mute a user.")]
        public async Task Mute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, null);
            await AddConfirmationEmoji();
        }

        [Command("tempmute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public async Task TempMute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The duration of the mute.")]
                string durationString,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            // TODO: Remove when we port to 2.0
            var duration = TimeSpanTypeReader.Read(durationString);
            if (!duration.HasValue) { throw new ArgumentException("Invalid Timespan Format"); }

            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, duration.Value);
            await AddConfirmationEmoji();
        }

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public async Task UnMute(
            [Summary("The user to be un-muted.")]
                IGuildUser subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.Id);
        }

        [Command("ban")]
        [Summary("Ban a user from the current guild.")]
        public async Task Ban(
            [Summary("The user to be banned.")]
                IGuildUser subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Ban, subject.Id, reason, null);
            await AddConfirmationEmoji();
        }

        [Command("forceban")]
        [Alias("ban")]
        [Summary("Ban a user from the guild, even if they are not a member.")]
        [Priority(10)]
        public async Task Forceban(
            [Summary("The id of the user to be banned.")]
                ulong subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Ban, subject, reason, null);
            await AddConfirmationEmoji();
        }

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public async Task UnBan(
            [Summary("The user to be un-banned.")]
                IGuildUser subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Ban, subject.Id);
            await AddConfirmationEmoji();
        }

        [Command("forceunban")]
        [Alias("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        [Priority(10)]
        public async Task ForceUnban(
            [Summary("The id of the user to be un-banned.")]
                ulong subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Ban, subject);
            await AddConfirmationEmoji();
        }

        internal protected IModerationService ModerationService { get; }
    }
}
