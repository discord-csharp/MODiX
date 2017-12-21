using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Quote;
using Serilog;

namespace Modix.Modules
{
    [Name("Quote Message"), Summary("Quote a message from the Guild with its ID (and optionally channel)")]
    public class QuoteModule : ModuleBase
    {
        private readonly IQuoteService _quoteService;

        private const int MAX_PARAMETER_LENGTH = 2;
        private const int CHANNEL_PARAMETER_INDEX = 0;
        private const int MESSAGE_PARAMETER_INDEX = 1;

        public QuoteModule(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [Command("quote"), Summary("Quote a message using its Discord ID, returns a pretty embed of the message along with the author")]
        public async Task Run([Remainder] string commandContent)
        {
            IMessage message = null;

            if (string.IsNullOrWhiteSpace(commandContent))
                return; // Do nothing

            var parameters = GetParameters(commandContent);

            try
            {
                if (parameters.Length == MAX_PARAMETER_LENGTH)
                {
                    message = await ProcessParameterQuoteSearch(parameters);
                }
                else
                {
                    message = await ProcessParameterlessSearch(commandContent);
                }
            }
            catch (CommandException ex)
            {
                Log.Error(ex, "Quote command ran by {User}, with parameter(s) {Parameters}, failed fetching either the channel or message", Context.User.Mention, commandContent);
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

        private async Task<IMessage> ProcessParameterQuoteSearch(string[] parameters)
        {
            var channelParameter = SanitiseChannelIdString(parameters[CHANNEL_PARAMETER_INDEX]);
            var messageParameter = parameters[MESSAGE_PARAMETER_INDEX];

            var isValidChannel = ulong.TryParse(channelParameter, out var channelId);
            var isValidMessage = ulong.TryParse(messageParameter, out var messageId);

            if (!isValidChannel || !isValidMessage)
                return null;

            var channel = await GetChannel(channelId);

            if (channel != null)
                return await GetMessage(messageId, channel);

            return null;
        }

        private async Task<IMessage> ProcessParameterlessSearch(string commandContent)
        {
            if (ulong.TryParse(commandContent, out var messageId))
                return await GetMessageFromId(messageId);

            return null;
        }

        private Task<ITextChannel> GetChannel(ulong channelId)
            => Context.Guild.GetTextChannelAsync(channelId);

        private Task<IMessage> GetMessage(ulong messageId, ITextChannel channel)
            => GetMessageInChannel(messageId, channel);

        private async Task<IMessage> GetMessageFromId(ulong messageId)
        {
            // Attempt to get the message from the channel the user
            // is executing the command from
            var message = await GetMessage(messageId, Context.Channel as ITextChannel);

            if (message == null)
            {
                // We haven't found a message, now fetch all text
                // channels and attempt to find the message

                var channels = await Context.Guild.GetTextChannelsAsync();

                foreach (var channel in channels)
                {
                    message = await GetMessage(messageId, channel);

                    if (message != null)
                        break;
                }
            }

            return message;
        }

        private static Task<IMessage> GetMessageInChannel(ulong messageId, ITextChannel channel) => channel.GetMessageAsync(messageId);

        private Task ReplyFailure() => ReplyAsync($"I couldn't find the message you're referring to {Context.User.Mention}");

        private static string[] GetParameters(string flatParameters) => flatParameters.Split(' ');

        private static string SanitiseChannelIdString(string unsanitisedInput) =>
            unsanitisedInput.Replace("<#", string.Empty).Replace(">", string.Empty);
    }
}
