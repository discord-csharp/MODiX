using Discord;

namespace Modix.Services.Quote
{
    public static class QuoteService
    {
        public static EmbedBuilder BuildQuoteEmbed(IMessage message)
        {
            return new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithDescription(message.Content)
                .WithFooter(GetPostedMeta(message))
                .WithColor(new Color(95, 186, 125));
        }

        private static string GetPostedMeta(IMessage message)
            => string.Format("{0:dddd, dd}{1} {0:MMMM yyyy} at {0:HH:mm}, in {2}", message.Timestamp, GetDaySuffix(message.Timestamp.Day), $"#{message.Channel.Name}");

        private static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}
