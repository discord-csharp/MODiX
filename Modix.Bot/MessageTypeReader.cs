using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Core;

namespace Modix
{
    public class MessageTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (ulong.TryParse(input, out var messageId))
            {
                var messageService = services.GetRequiredService<IMessageService>();

                var message = await messageService.FindMessageAsync(context.Guild.Id, messageId);

                if (message is { })
                    return TypeReaderResult.FromSuccess(message);
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
        }
    }
}
