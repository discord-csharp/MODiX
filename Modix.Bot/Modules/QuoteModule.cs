using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Quote;
using Serilog;

namespace Modix.Modules
{
    [Name("Quoting"), Summary("Quote a message from the guild with its ID.")]
    public class QuoteModule : ModuleBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteModule(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [Command("quote"), Summary("Quote the given message.")]
        public async Task RunAsync(
            [Summary("The ID of the message to quote.")]
                ulong messageId)
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

            await ProcessRetrievedMessageAsync(message);
        }

        [Command("quote"), Summary("Quote the given message from the given channel.")]
        public async Task RunAsync(
            [Summary("The channel in which the message resides.")]
                ITextChannel channel,
            [Summary("The ID of the message to quote.")]
                ulong messageId)
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

            await ProcessRetrievedMessageAsync(message);
        }

        [Command("quote"), Summary("Quote the given message from the given channel.")]
        public async Task RunAsync(
            [Summary("The ID of the message to quote.")]
                ulong messageId,
            [Summary("The channel in which the message resides.")]
                ITextChannel channel)
            => await RunAsync(channel, messageId);

        private async Task ProcessRetrievedMessageAsync(IMessage message)
        {
            if (message != null)
            {
                await _quoteService.BuildRemovableEmbed(message, Context.User,
                    async (embed) => await ReplyAsync(embed: embed.Build()));
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
                catch (Exception e)
                {
                    Log.Debug(e, "Failed accessing channel {ChannelName} when searching for message {MessageId}",
                        channel.Name, messageId);
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
