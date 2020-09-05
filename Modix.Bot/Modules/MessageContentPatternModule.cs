using System.Threading.Tasks;
using Discord.Commands;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.Blocklist;

namespace Modix.Bot.Modules
{
    [Name("Patterns")]
    [Summary("Use and maintain message content patterns.")]
    [Group("pattern")]
    [Alias("patterns")]
    public class MessageContentPatternModule : ModuleBase
    {
        private readonly IMessageContentPatternService _messageContentPatternService;

        public MessageContentPatternModule(IMessageContentPatternService messageContentPatternService)
        {
            _messageContentPatternService = messageContentPatternService;
        }

        [Command("block")]
        [Summary("Adds a new pattern to block.")]
        public async Task BlockAsync([Summary("Regex pattern for the blocked content."), Remainder] string pattern)
        {
            if (await _messageContentPatternService.DoesPatternExist(Context.Guild.Id, pattern))
            {
                await ReplyAsync("This pattern has already been added!");
                return;
            }

            var response = await _messageContentPatternService.AddPattern(Context.Guild.Id, pattern, MessageContentPatternType.Blocked);

            if (response.Failure)
            {
                await ReplyAsync(response.ErrorMessage);
                return;
            }

            await Context.AddConfirmation();
        }

        [Command("allow")]
        [Summary("Adds a new pattern to allow, superseding any blocks.")]
        public async Task AllowAsync([Summary("Regex pattern for the allowed content."), Remainder] string pattern)
        {
            if (await _messageContentPatternService.DoesPatternExist(Context.Guild.Id, pattern))
            {
                await ReplyAsync("This pattern has already been added!");
                return;
            }

            var response = await _messageContentPatternService.AddPattern(Context.Guild.Id, pattern, MessageContentPatternType.Allowed);

            if (response.Failure)
            {
                await ReplyAsync(response.ErrorMessage);
                return;
            }

            await Context.AddConfirmation();
        }

        [Command("remove")]
        [Alias("delete")]
        [Summary("Removes pattern from any message checks.")]
        public async Task RemoveAsync([Summary("Regex pattern to be removed."), Remainder] string pattern)
        {
            var response = await _messageContentPatternService.RemovePattern(Context.Guild.Id, pattern);

            if (response.Failure)
            {
                await ReplyAsync(response.ErrorMessage);
                return;
            }

            await Context.AddConfirmation();
        }
    }
}
