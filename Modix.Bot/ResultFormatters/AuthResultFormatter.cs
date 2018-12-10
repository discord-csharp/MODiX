using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Modix.Services.Core;
using Modix.Services.ErrorHandling;

namespace Modix.Bot.ResultFormatters
{
    public class AuthResultFormatter : DiscordResultFormatter<AuthResult>
    {
        /// <summary>
        /// Formats a <see cref="AuthResult"/> for sending to Discord
        /// </summary>
        /// <param name="contextAccessor"></param>
        public AuthResultFormatter(ICommandContextAccessor contextAccessor) : base(contextAccessor) { }

        public override EmbedBuilder Format(AuthResult result)
        {
            var listBuilder = new StringBuilder();

            //Order so we have the claims the user actually had first
            foreach (var status in result.RequiredClaims.OrderByDescending(d => result.HadRequiredClaim(d)))
            {
                listBuilder.Append(result.HadRequiredClaim(status) ? "✅ " : "❌ ");
                listBuilder.AppendLine(status.ToString());
            }

            return new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithTitle("Sorry, you don't have the claims for that.")
                .WithDescription(Context.Message.Content)
                .WithColor(new Color(255, 0, 0))
                .AddField("Claim Status", listBuilder.ToString())
                .WithTimestamp(DateTimeOffset.UtcNow);
        }
    }
}
