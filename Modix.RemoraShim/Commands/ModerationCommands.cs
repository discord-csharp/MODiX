using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
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
            IDiscordRestChannelAPI channelApi,
            ICommandContext context,
            ICommandConfirmationService commandConfirmationService,
            IModerationService moderationService,
            IUserService userService)
        {
            _channelApi = channelApi;
            _context = context;
            _commandConfirmationService = commandConfirmationService;
            _moderationService = moderationService;
            _userService = userService;
        }

        [Command("note")]
        public async Task<IResult> NoteAsync(UserOrMessageAuthor subject, [Greedy] string reason)
        {
            var channelResult = await _channelApi.GetChannelAsync(_context.ChannelID);
            if (!channelResult.IsSuccess || channelResult.Entity is { ThreadMetadata: { HasValue: false } })
                return Result.FromError(channelResult);

            var confirmationResult = await GetConfirmationIfRequiredAsync(subject);
            if (!confirmationResult.IsSuccess || !confirmationResult.Entity)
                return Result.FromSuccess();

            try
            {
                var reasonWithUrls = AppendUrlsFromMessage(reason);
                await _moderationService.CreateInfractionAsync(_context.GuildID.Value.Value, _context.User.ID.Value, InfractionType.Notice, subject.User.ID.Value, reasonWithUrls, duration: null);
                return await ConfirmAsync();
            }
            catch (Exception ex)
            {
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

        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ICommandContext _context;
        private readonly ICommandConfirmationService _commandConfirmationService;
        private readonly IModerationService _moderationService;
        private readonly IUserService _userService;
    }
}
