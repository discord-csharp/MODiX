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
            ModerationService = moderationService ?? throw new ArgumentNullException(nameof(moderationService));
        }

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public Task Note(
            [Summary("The user to which the note is being applied.")]
                IGuildUser subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Notice, subject.Id, reason, null);

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public Task Warn(
            [Summary("The user to which the warning is being issued.")]
                IGuildUser subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Warning, subject.Id, reason, null);

        [Command("mute")]
        [Summary("Mute a user.")]
        public Task Mute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, null);

        [Command("mute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public Task TempMute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The duration of the mute.")]
                TimeSpan duration,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, duration);

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public Task UnMute(
            [Summary("The user to be un-muted.")]
                IGuildUser subject)
            => ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.Id);

        [Command("ban")]
        [Summary("Ban a user from the current guild.")]
        public Task Ban(
            [Summary("The user to be banned.")]
                IGuildUser subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Ban, subject.Id, reason, null);

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public Task UnBan(
            [Summary("The user to be un-banned.")]
                IGuildUser subject)
            => ModerationService.RescindInfractionAsync(InfractionType.Ban, subject.Id);

        internal protected IModerationService ModerationService { get; }
    }
}
