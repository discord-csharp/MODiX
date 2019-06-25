using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Core;

namespace Modix.Bot
{
    public class AnyGuildMessageTypeReader<TMessage>
        : TypeReader
        where TMessage : IMessage
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!ulong.TryParse(input, out var messageId))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid Message ID");

            var messageService = services.GetRequiredService<IMessageService>();
            var message = await messageService.FindMessageAsync(context.Guild.Id, messageId);

            if (message is null)
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found. Are you sure I have permission to read the appropriate channels and message histories?");

            if (!(message is TMessage result))
                return TypeReaderResult.FromError(CommandError.Exception, "Incompatible message.");

            return TypeReaderResult.FromSuccess(AnyGuildMessage.FromMessage(result));
        }
    }
}
