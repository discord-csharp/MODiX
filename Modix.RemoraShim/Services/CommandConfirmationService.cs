using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Services
{
    public interface ICommandConfirmationService
    {
        Task<IResult> AddConfirmationAsync(Snowflake channelId, Snowflake messageId);
        Task<Result<bool>> GetUserConfirmationAsync(Snowflake channelId, Snowflake userId, string mainMessage);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class CommandConfirmationService
        : ICommandConfirmationService
    {
        public CommandConfirmationService(
            IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public async Task<IResult> AddConfirmationAsync(Snowflake channelId, Snowflake messageId)
            => await _channelApi.CreateReactionAsync(channelId, messageId, ConfirmButtonEmoji);

        public async Task<Result<bool>> GetUserConfirmationAsync(Snowflake channelId, Snowflake userId, string mainMessage)
        {
            if (!mainMessage.EndsWith(Environment.NewLine))
                mainMessage += Environment.NewLine;

            var confirmationMessageSendResult = await _channelApi.CreateMessageAsync(channelId, mainMessage +
                $"React with {ConfirmButtonEmoji} or {CancelButtonEmoji} in the next {ConfirmationTimeoutSeconds} seconds to finalize or cancel the operation.");

            if (!confirmationMessageSendResult.IsSuccess || confirmationMessageSendResult.Entity is not IMessage confirmationMessage)
                return Result<bool>.FromError(confirmationMessageSendResult);

            var createConfirmButtonResult = await _channelApi.CreateReactionAsync(channelId, confirmationMessage.ID, ConfirmButtonEmoji);
            if (!createConfirmButtonResult.IsSuccess)
                return Result<bool>.FromError(createConfirmButtonResult);

            var createCancelButtonResult = await _channelApi.CreateReactionAsync(channelId, confirmationMessage.ID, CancelButtonEmoji);
            if (!createCancelButtonResult.IsSuccess)
                return Result<bool>.FromError(createCancelButtonResult);

            for (var i = 0; i < ConfirmationTimeoutSeconds; i++)
            {
                await Task.Delay(1000);

                var denyingUsersResult = await _channelApi.GetReactionsAsync(channelId, confirmationMessage.ID, CancelButtonEmoji);
                if (!denyingUsersResult.IsSuccess)
                    return Result<bool>.FromError(denyingUsersResult);

                if (denyingUsersResult.Entity.Any(u => u.ID == userId))
                {
                    await RemoveReactionsAndUpdateMessage("Cancellation was successfully received. Cancelling the operation.");
                    return false;
                }

                var confirmingUsersResult = await _channelApi.GetReactionsAsync(channelId, confirmationMessage.ID, ConfirmButtonEmoji);
                if (!confirmingUsersResult.IsSuccess)
                    return Result<bool>.FromError(confirmingUsersResult);

                if (confirmingUsersResult.Entity.Any(u => u.ID == userId))
                {
                    await RemoveReactionsAndUpdateMessage("Confirmation was successfully received. Performing the operation.");
                    return true;
                }
            }

            await RemoveReactionsAndUpdateMessage("Confirmation was not received. Cancelling the operation.");
            return false;

            async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
            {
                await _channelApi.DeleteAllReactionsAsync(channelId, confirmationMessage.ID);
                await _channelApi.EditMessageAsync(channelId, confirmationMessage.ID, mainMessage + bottomMessage);
            }
        }

        private readonly IDiscordRestChannelAPI _channelApi;

        private const string CancelButtonEmoji
            = "❌";
        private const string ConfirmButtonEmoji
            = "✅";
        private const int ConfirmationTimeoutSeconds
            = 10;
    }
}
