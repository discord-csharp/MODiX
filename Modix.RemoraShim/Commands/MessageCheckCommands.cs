using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.RemoraShim.Models;
using Modix.RemoraShim.Parsers;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.MessageContentPatterns;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Commands
{
    [Group("pattern", "patterns")]
    public class MessageCheckCommands
        : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly ICommandConfirmationService _commandConfirmationService;
        private readonly IMessageContentPatternService _messageContentPatternService;
        private readonly IUserService _userService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;


        public MessageCheckCommands(
            ICommandContext context,
            ICommandConfirmationService commandConfirmationService,
            IMessageContentPatternService messageContentPatternService,
            IUserService userService,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestGuildAPI guildApi)
        {
            _context = context;
            _commandConfirmationService = commandConfirmationService;
            _messageContentPatternService = messageContentPatternService;
            _userService = userService;
            _channelApi = channelApi;
            _guildApi = guildApi;
        }

        [Command("list")]
        public async Task<IResult> ListAsync()
        {
            var canViewPatterns = _messageContentPatternService.CanViewPatterns();
            if(_context is not MessageContext)
            {
                return Result.FromError(new InvalidOperationError("Not a message context"));
            }

            if (!canViewPatterns)
            {
                await _channelApi.CreateMessageAsync(_context.ChannelID, $"<@!{_context.User.ID}> does not have permission to view patterns blocked or allowed in guild {_context.GuildID.Value.Value}!", allowedMentions: new NoAllowedMentions());
                return Result.FromError(new InvalidOperationError("You do not have permission to view patterns blocked or allowed in this guild!"));
            }

            var patterns = await _messageContentPatternService.GetPatterns(_context.GuildID.Value.Value);

            if (!patterns.Any())
            {
                await _channelApi.CreateMessageAsync(_context.ChannelID, $"Guild {_context.GuildID.Value.Value} does not have any patterns set up, get started with `!pattern block` or `!pattern allow`");
                return Result.FromError(new InvalidOperationError("This guild does not have any patterns set up, get started with `!pattern block` or `!pattern allow`"));
            }

            var blocked = patterns.Any(x => x.Type == MessageContentPatternType.Blocked)
                ? string.Join(Environment.NewLine, patterns.Where(x => x.Type == MessageContentPatternType.Blocked).Select(x => $"- `{x.Pattern}`"))
                : "There are no blocked patterns";

            var allowed = patterns.Any(x => x.Type == MessageContentPatternType.Allowed)
                ? string.Join(Environment.NewLine, patterns.Where(x => x.Type == MessageContentPatternType.Allowed).Select(x => $"- `{x.Pattern}`"))
                : "There are no allowed patterns";
            var guild = await _guildApi.GetGuildAsync(_context.GuildID.Value);

            var embed = new Embed(
                Title: $"Message Patterns for {guild.Entity!.Name}",
                Description: new Optional<string>("Allowed patterns supersede those that are blocked."),
                Fields: Enumerable.Empty<IEmbedField>().Concat(blocked.EnumerateLongTextAsFieldBuilders("Blocked"))
                    .Concat(allowed.EnumerateLongTextAsFieldBuilders("Allowed")).ToList()
                );

            await _channelApi.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
            return Result.FromSuccess();
        }

        [Command("block")]
        public async Task<IResult> BlockAsync([Greedy] string pattern)
        {
            var response = await _messageContentPatternService.AddPattern(_context.GuildID.Value.Value, pattern, MessageContentPatternType.Blocked);

            if (response.Failure)
            {
                return Result.FromError(new InvalidOperationError(response.ErrorMessage));
            }

            return await ConfirmAsync();
        }


        [Command("allow")]
        public async Task<IResult> AllowAsync([Greedy] string pattern)
        {
            var response = await _messageContentPatternService.AddPattern(_context.GuildID.Value.Value, pattern, MessageContentPatternType.Allowed);

            if (response.Failure)
            {
                return Result.FromError(new InvalidOperationError(response.ErrorMessage));
            }

            return await ConfirmAsync();
        }

        [Command("remove", "delete")]
        public async Task<IResult> RemoveAsync([Greedy] string pattern)
        {
            var response = await _messageContentPatternService.RemovePattern(_context.GuildID.Value.Value, pattern);

            if (response.Failure)
            {
                return Result.FromError(new InvalidOperationError(response.ErrorMessage));
            }
            return await ConfirmAsync();
        }


        private async ValueTask<IResult> ConfirmAsync()
        {
            if (_context is not MessageContext messageContext)
                return Result.FromSuccess();

            return await _commandConfirmationService.AddConfirmationAsync(_context.ChannelID, messageContext.MessageID);
        }

    }
}
