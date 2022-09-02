using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Serilog;

namespace Modix.Bot.Extensions
{
    public static class ContextExtensions
    {
        private static readonly Emoji _checkmarkEmoji = new("✅");
        private static readonly Emoji _xEmoji = new("❌");

        private const int ConfirmationTimeoutSeconds = 10;

        public static async Task AddConfirmationAsync(this ICommandContext context)
        {
            if (context.Channel is not IGuildChannel guildChannel)
            {
                return;
            }

            var currentUser = await context.Guild.GetCurrentUserAsync();
            var permissions = currentUser.GetPermissions(guildChannel);

            if (!permissions.AddReactions)
            {
                Log.Information("Unable to add a confirmation reaction in {0}, because the AddReactions permission is denied.", guildChannel.Name);
                return;
            }

            await context.Message.AddReactionAsync(_checkmarkEmoji);
        }

        public static async Task AddConfirmationAsync(this IInteractionContext context)
            => await context.Interaction.FollowupAsync($"\\{_checkmarkEmoji} Command successful.");

        public static async Task<bool> GetUserConfirmationAsync(this ICommandContext context, string mainMessage)
        {
            if (context.Channel is not IGuildChannel guildChannel)
            {
                return false;
            }

            var currentUser = await context.Guild.GetCurrentUserAsync();
            var permissions = currentUser.GetPermissions(guildChannel);

            if (!permissions.AddReactions)
            {
                throw new InvalidOperationException("Unable to get user confirmation, because the AddReactions permission is denied.");
            }

            if (!mainMessage.EndsWith(Environment.NewLine))
                mainMessage += Environment.NewLine;

            var confirmationMessage = await context.Channel.SendMessageAsync(mainMessage +
                $"React with {_checkmarkEmoji} or {_xEmoji} in the next {ConfirmationTimeoutSeconds} seconds to finalize or cancel the operation.");

            await confirmationMessage.AddReactionAsync(_checkmarkEmoji);
            await confirmationMessage.AddReactionAsync(_xEmoji);

            for (var i = 0; i < ConfirmationTimeoutSeconds; i++)
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

            await RemoveReactionsAndUpdateMessage("Confirmation was not received. Cancelling the operation.");
            return false;

            async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
            {
                await confirmationMessage.RemoveAllReactionsAsync();
                await confirmationMessage.ModifyAsync(m => m.Content = mainMessage + bottomMessage);
            }
        }
    }
}
