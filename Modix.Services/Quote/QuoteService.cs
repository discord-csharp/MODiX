using System;
using Discord;
using static System.FormattableString;

namespace Modix.Services.Quote
{
    public interface IQuoteService
    {
        EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser);
    }

    public class QuoteService : IQuoteService
    {
        public EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser)
        {
            string messageUrl = null;
            if (message.Channel is IGuildChannel guildChannel && guildChannel.Guild is IGuild guild)
            {
                messageUrl = Invariant($"https://discordapp.com/channels/{guild.Id}/{guildChannel.Id}/{message.Id}");
            }

            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x = x.WithIconUrl(message.Author.GetAvatarUrl()).WithName(message.Author.Username);

                    if (messageUrl != null)
                    {
                        x.WithUrl(messageUrl);
                    }
                })
                .WithDescription(message.Content);

            embed.AddField("Quoted by", executingUser.Mention, true);

            embed.WithFooter(GetPostedMeta(message)).WithColor(new Color(95, 186, 125));

            return embed;
        }

        private static string GetPostedMeta(IMessage message)
            => string.Format("{0:dddd, dd}{1} {0:MMMM yyyy} at {0:HH:mm}, in #{2}", message.Timestamp, GetDaySuffix(message.Timestamp.Day), message.Channel.Name);

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
