using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Quote;
using Serilog;

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

        [Command("quote")]
        public async Task Run(ulong messageId)
        {
            IMessage message = null;

            try
            {
                message = await GetMessage(messageId, Context.Channel as ITextChannel);

                if (message == null)
                    message = await FindMessageInUnknownChannel(messageId);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed fetching message for Quote command, ran by {User} with a Message ID of {MessageId}", 
                    Context.User.Mention, messageId);
            }

            await ProcessRetrievedMessage(message);
        }

        [Command("quote")]
        public async Task Run(ITextChannel channel, ulong messageId)
        {
            IMessage message = null;

            try
            {
                message = await GetMessage(messageId, channel);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed fetching message for Quote command, ran by {User} with a Message ID of {MessageId} for channel {Channel}", 
                    Context.User.Mention, messageId, channel.Name);
            }

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

        private async Task ProcessRetrievedMessage(IMessage message)
        {
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

        private async Task<IMessage> FindMessageInUnknownChannel(ulong messageId)
        {
            IMessage message = null;

            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = await Context.Guild.GetTextChannelsAsync();

            foreach (var channel in channels)
            {
                try
                {
                    message = await GetMessage(messageId, channel);

                    if (message != null)
                        break;
                }
                catch (CommandException e)
                {
                    Log.Error(e, "Failed fetching message for Quote command, ran by {User} with a Message ID of {MessageId}",
                        Context.User.Mention, messageId);
                }
            }

            return message;
        }

        private Task<IMessage> GetMessage(ulong messageId, ITextChannel channel)
            => GetMessageInChannel(messageId, channel);

        private static Task<IMessage> GetMessageInChannel(ulong messageId, ITextChannel channel) 
            => channel.GetMessageAsync(messageId);

        private Task ReplyFailure() => ReplyAsync($"I couldn't find the message you're referring to {Context.User.Mention}");
    }
}
