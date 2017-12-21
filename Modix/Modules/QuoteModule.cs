using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Quote;

namespace Modix.Modules
{
    [Name("Quote Message"), Summary("Quote a message from the Guild with its ID")]
    public class QuoteModule : ModuleBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteModule(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [Command("quote"), Summary("Quote a message using its Discord ID, returns a pretty embed of the message along with the author")]
        public async Task Run([Remainder] string id)
        {
            IMessage message = null;

            if (ulong.TryParse(id, out var messageId))
                message = await GetMessage(messageId);

            if (message != null)
            {
                var quoteEmbed = _quoteService.BuildQuoteEmbed(message);

                await ReplyAsync(string.Empty, false, quoteEmbed);
            }
            else
            {
                await ReplyFailure();
            }

            // Delete the message originally sent, we're done (even if we've failed fetching)
            await Context.Message.DeleteAsync();
        }

        private async Task<IMessage> GetMessage(ulong messageId)
        {
            // Attempt to get the message from the channel the user
            // is executing the command from
            var message = await GetMessageInChannel(messageId);

            if (message == null)
            {
                // We haven't found a message, now fetch all text
                // channels and attempt to find the message

                var channels = await Context.Guild.GetTextChannelsAsync();

                foreach (var channel in channels)
                {
                    message = await GetMessageInChannel(messageId, channel);

                    if (message != null)
                        break;
                }
            }

            return message;
        }

        private Task<IMessage> GetMessageInChannel(ulong messageId) => Context.Channel.GetMessageAsync(messageId);

        private Task<IMessage> GetMessageInChannel(ulong messageId, ITextChannel channel) => channel.GetMessageAsync(messageId);

        private Task ReplyFailure() => ReplyAsync($"I couldn't find the message you're referring to {Context.User.Mention}");
    }
}
