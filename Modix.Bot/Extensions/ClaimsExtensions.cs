using System.Collections.Generic;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.Bot.Extensions
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Turns a list of <see cref="AuthorizationClaim" /> into a newlined list of claims.
        /// </summary>
        /// <param name="claims">Claims to turn into strings and add to the message</param>
        /// <returns>Claims as a string list, newlined.</returns>
        public static string ToMessageableList(this IReadOnlyCollection<AuthorizationClaim> claims)
        {
            var message = new StringBuilder();

            foreach (var claim in claims)
            {
                message.Append(claim.ToString());
                message.AppendLine();
            }

            return message.ToString();
        }
    }
}
