using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Modix.Bot.Extensions
{
    public static class CommandContextExtensions
    {
        private static readonly Emoji _checkmarkEmoji = new Emoji("✅");
        private static readonly Emoji _xEmoji = new Emoji("❌");
        private const int _confirmationTimeoutSeconds = 10;

        public static Task AddConfirmation(this ICommandContext context)
            => context.Message.AddReactionAsync(_checkmarkEmoji);

        public static async Task<bool> GetUserConfirmationAsync(this ICommandContext context, string mainMessage)
        {
            if (!mainMessage.EndsWith(Environment.NewLine))
                mainMessage += Environment.NewLine;

            var confirmationMessage = await context.Channel.SendMessageAsync(mainMessage +
                $"React with {_checkmarkEmoji} or {_xEmoji} in the next {_confirmationTimeoutSeconds} seconds to finalize or cancel the operation.");

            await confirmationMessage.AddReactionAsync(_checkmarkEmoji);
            await confirmationMessage.AddReactionAsync(_xEmoji);

            for (var i = 0; i < _confirmationTimeoutSeconds; i++)
            {
                await Task.Delay(1000);

                var denyingUsers = await confirmationMessage.GetReactionUsersAsync(_xEmoji, int.MaxValue).FlattenAsync();
                if (denyingUsers.Any(u => u.Id == context.User.Id))
                {
                    await RemoveReactionsAndUpdateMessage("Cancellation was successfully received. Cancelling the operation.");
                    return false;
                }

                var confirmingUsers = await confirmationMessage.GetReactionUsersAsync(_checkmarkEmoji, int.MaxValue).FlattenAsync();
                if (confirmingUsers.Any(u => u.Id == context.User.Id))
                {
                    await RemoveReactionsAndUpdateMessage("Confirmation was successfully received. Performing the operation.");
                    return true;
                }
            }

            await RemoveReactionsAndUpdateMessage("Confirmation was not received. Cancelling deletion.");
            return false;

            async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
            {
                await confirmationMessage.RemoveAllReactionsAsync();
                await confirmationMessage.ModifyAsync(m => m.Content = mainMessage + bottomMessage);
            }
        }
    }
}
