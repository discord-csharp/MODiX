using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix
{
    public class DiscordUserMessageTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (ulong.TryParse(input, out var messageId))
            {
                var value = await FindMessageInUnknownChannelAsync(context, messageId, services);
                var userMessage = value as IUserMessage;

                if (userMessage is null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");

                return TypeReaderResult.FromSuccess(new DiscordUserMessage(userMessage));
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
        }

        // TODO: extract to service layer after the PR that adds the DB check to the quote service version of this is merged.
        private async Task<IMessage> FindMessageInUnknownChannelAsync(ICommandContext context, ulong messageId, IServiceProvider services)
        {
            var messageRepository = services.GetService<IMessageRepository>();
            var guildMessage = await messageRepository.GetMessage(messageId);

            if (guildMessage is { })
            {
                var channel = await context.Guild.GetTextChannelAsync(guildMessage.ChannelId);
                return await channel.GetMessageAsync(messageId);
            }

            IMessage message = null;

            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = await context.Guild.GetTextChannelsAsync();

            var selfUserProvider = services.GetService<ISelfUserProvider>();
            var selfUser = await selfUserProvider.GetSelfUserAsync();
            var selfGuildUser = await context.Guild.GetUserAsync(selfUser.Id);

            foreach (var channel in channels)
            {
                if (selfGuildUser.GetPermissions(channel).ReadMessageHistory)
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message is { })
                        break;
                }
            }

            return message;
        }
    }

    public class DiscordUserMessage
    {
        public DiscordUserMessage(IUserMessage userMessage)
        {
            _userMessage = userMessage;
        }

        public IUserMessage ToUserMessage()
            => _userMessage;

        private readonly IUserMessage _userMessage;
    }
}
