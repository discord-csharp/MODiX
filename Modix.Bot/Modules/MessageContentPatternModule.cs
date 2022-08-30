using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.MessageContentPatterns;

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

        [Command("list")]
        [Summary("Lists all added patterns.")]
        public async Task ListAsync()
        {
            var canViewPatterns = _messageContentPatternService.CanViewPatterns();

            if (!canViewPatterns)
            {
                await ReplyAsync("You do not have permission to view patterns blocked or allowed in this guild!");
                return;
            }

            var patterns = await _messageContentPatternService.GetPatterns(Context.Guild.Id);

            if (!patterns.Any())
            {
                await ReplyAsync("This guild does not have any patterns set up, get started with `!pattern block` or `!pattern allow`");
                return;
            }

            var blocked = patterns.Any(x => x.Type == MessageContentPatternType.Blocked)
                ? string.Join(Environment.NewLine, patterns.Where(x => x.Type == MessageContentPatternType.Blocked).Select(x => $"- `{x.Pattern}`"))
                : "There are no blocked patterns";

            var allowed = patterns.Any(x => x.Type == MessageContentPatternType.Allowed)
                ? string.Join(Environment.NewLine, patterns.Where(x => x.Type == MessageContentPatternType.Allowed).Select(x => $"- `{x.Pattern}`"))
                : "There are no allowed patterns";

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Message Patterns for {Context.Guild.Name}")
                .WithDescription("Allowed patterns supersede those that are blocked.")
                .AddField("Blocked", blocked)
                .AddField("Allowed", allowed);

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("block")]
        [Summary("Adds a new pattern to block.")]
        public async Task BlockAsync([Summary("Regex pattern for the blocked content."), Remainder] string pattern)
        {
            var response = await _messageContentPatternService.AddPattern(Context.Guild.Id, pattern, MessageContentPatternType.Blocked);

            if (response.Failure)
            {
                await ReplyAsync(response.ErrorMessage);
                return;
            }

            await Context.AddConfirmationAsync();
        }

        [Command("allow")]
        [Summary("Adds a new pattern to allow, superseding any blocks.")]
        public async Task AllowAsync([Summary("Regex pattern for the allowed content."), Remainder] string pattern)
        {
            var response = await _messageContentPatternService.AddPattern(Context.Guild.Id, pattern, MessageContentPatternType.Allowed);

            if (response.Failure)
            {
                await ReplyAsync(response.ErrorMessage);
                return;
            }

            await Context.AddConfirmationAsync();
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

            await Context.AddConfirmationAsync();
        }
    }
}
