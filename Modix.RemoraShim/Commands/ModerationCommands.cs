using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;
using Modix.RemoraShim.Models;
using Modix.RemoraShim.Parsers;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Modix.RemoraShim.Commands
{
    public class ModerationCommands
        : CommandGroup
    {
        public ModerationCommands(
            ICommandContext context,
            ICommandConfirmationService commandConfirmationService,
            IDiscordRestChannelAPI channelApi,
            IModerationService moderationService,
            IUserService userService)
        {
            _context = context;
            _commandConfirmationService = commandConfirmationService;
            _channelApi = channelApi;
            _moderationService = moderationService;
            _userService = userService;
        }

        [Command("note")]
        public async Task<IResult> NoteAsync(UserOrMessageAuthor subject, [Greedy] string reason)
            => await CreateInfractionAsync(subject, reason, InfractionType.Notice);

        [Command("warn")]
        public async Task<IResult> WarnAsync(UserOrMessageAuthor subject, [Greedy] string reason)
            => await CreateInfractionAsync(subject, reason, InfractionType.Warning);

        [Command("mute", "tempmute")]
        public async Task<IResult> MuteAsync(UserOrMessageAuthor subject, TimeSpan duration, [Greedy] string reason)
            => await CreateInfractionAsync(subject, reason, InfractionType.Mute, duration);

        [Command("mute", "tempmute")]
        public async Task<IResult> MuteAsync(TimeSpan duration, UserOrMessageAuthor subject, [Greedy] string reason)
            => await CreateInfractionAsync(subject, reason, InfractionType.Mute, duration);

        [Command("unmute")]
        public async Task<IResult> UnmuteAsync(UserOrMessageAuthor subject, [Greedy] string reason = "")
            => await RescindInfractionAsync(subject, reason, InfractionType.Mute);

        [Command("ban", "forceban")]
        public async Task<IResult> BanAsync(UserOrMessageAuthor subject, [Greedy] string reason)
            => await CreateInfractionAsync(subject, reason, InfractionType.Ban);

        [Command("unban")]
        public async Task<IResult> UnbanAsync(UserOrMessageAuthor subject, [Greedy] string reason = "")
            => await RescindInfractionAsync(subject, reason, InfractionType.Ban);

        private async Task<IResult> CreateInfractionAsync(UserOrMessageAuthor subject, string reason, InfractionType infractionType, TimeSpan? duration = null)
        {
            var confirmationResult = await GetConfirmationIfRequiredAsync(subject);
            if (!confirmationResult.IsSuccess || !confirmationResult.Entity)
                return Result.FromSuccess();

            try
            {
                var reasonWithUrls = AppendUrlsFromMessage(reason);
                await _moderationService.CreateInfractionAsync(_context.GuildID.Value.Value, _context.User.ID.Value, infractionType, subject.User.ID.Value, reasonWithUrls, duration: duration);
                return await ConfirmAsync();
            }
            catch (Exception ex)
            {
                await _channelApi.CreateMessageAsync(_context.ChannelID, ex.Message, allowedMentions: new NoAllowedMentions());
                return Result.FromError(new ExceptionError(ex));
            }
        }

        private async Task<IResult> RescindInfractionAsync(UserOrMessageAuthor subject, string reason, InfractionType infractionType)
        {
            var confirmationResult = await GetConfirmationIfRequiredAsync(subject);
            if (!confirmationResult.IsSuccess || !confirmationResult.Entity)
                return Result.FromSuccess();

            try
            {
                var reasonWithUrls = AppendUrlsFromMessage(reason);
                await _moderationService.RescindInfractionAsync(infractionType, _context.GuildID.Value.Value, subject.User.ID.Value, reasonWithUrls);
                return await ConfirmAsync();
            }
            catch (Exception ex)
            {
                await _channelApi.CreateMessageAsync(_context.ChannelID, ex.Message, allowedMentions: new NoAllowedMentions());
                return Result.FromError(new ExceptionError(ex));
            }
        }

        private string AppendUrlsFromMessage(string reason)
        {
            if (_context is not MessageContext messageContext || !messageContext.Message.Attachments.HasValue)
                return reason;

            var urls = messageContext.Message.Attachments.Value
                .Select(x => x.Url)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            if (!urls.Any())
                return reason;

            return new StringBuilder(reason)
                .AppendLine()
                .AppendLine()
                .AppendJoin(Environment.NewLine, urls)
                .ToString();
        }

        private async ValueTask<Result<bool>> GetConfirmationIfRequiredAsync(UserOrMessageAuthor userOrAuthor)
        {
            if (userOrAuthor.MessageId is null)
                return true;

            var author = await _userService.GetUserAsync(userOrAuthor.User.ID.Value);

            Debug.Assert(author is not null); // author should be non-null, because we have a message written by someone with that ID

            return await _commandConfirmationService.GetUserConfirmationAsync(_context.ChannelID, _context.User.ID,
                "Detected a message ID instead of a user ID. Do you want to perform this action on "
                + $"**{author.GetFullUsername()}** ({userOrAuthor.User.ID.Value}), the message's author?");
        }

        private async ValueTask<IResult> ConfirmAsync()
        {
            if (_context is not MessageContext messageContext)
                return Result.FromSuccess();

            return await _commandConfirmationService.AddConfirmationAsync(_context.ChannelID, messageContext.MessageID);
        }

        private readonly ICommandContext _context;
        private readonly ICommandConfirmationService _commandConfirmationService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IModerationService _moderationService;
        private readonly IUserService _userService;
    }
}
