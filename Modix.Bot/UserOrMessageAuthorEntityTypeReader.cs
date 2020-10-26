using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Core;
using Modix.Services.Utilities;

using System;
using System.Threading.Tasks;

namespace Modix
{
    public class DiscordUserOrMessageAuthorEntity
    {
        public DiscordUserOrMessageAuthorEntity(ulong userId)
        {
            UserId = userId;
        }

        public DiscordUserOrMessageAuthorEntity(ulong userId, ulong messageChannelId, ulong messageId)
        {
            UserId = userId;
            MessageChannelId = messageChannelId;
            MessageId = messageId;
        }

        public ulong UserId { get; }

        public ulong? MessageChannelId { get; }

        public ulong? MessageId { get; }
    }

    public class UserOrMessageAuthorEntityTypeReader : UserTypeReader<IGuildUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            input = input.Trim(',', '.');

            var baseResult = await base.ReadAsync(context, input, services);

            if (baseResult.IsSuccess)
            {
                return TypeReaderResult.FromSuccess(new DiscordUserOrMessageAuthorEntity(((IUser)baseResult.BestMatch).Id));
            }

            if (MentionUtils.TryParseUser(input, out var userId))
            {
                return SnowflakeUtilities.IsValidSnowflake(userId)
                    ? GetUserResult(userId)
                    : GetInvalidSnowflakeResult();
            }

            // The base class covers users that are in the guild, and the previous condition covers mentioning users that are not in the guild.
            // At this point, it's one of the following:
            //   - A snowflake for a message in the guild.
            //   - A snowflake for a user not in the guild.
            //   - Something we can't handle.
            if (ulong.TryParse(input, out var snowflake))
            {
                if (!SnowflakeUtilities.IsValidSnowflake(snowflake))
                {
                    return GetInvalidSnowflakeResult();
                }

                var messageService = services.GetRequiredService<IMessageService>();

                if (await messageService.FindMessageAsync(context.Guild.Id, snowflake) is { } message)
                {
                    return GetMessageAuthorResult(message.Author.Id, message.Channel.Id, message.Id);
                }

                // At this point, our best guess is that the snowflake is for a user who is not in the guild.
                return GetUserResult(snowflake);
            }

            return GetBadInputResult();
        }

        private static TypeReaderResult GetUserResult(ulong userId)
            => TypeReaderResult.FromSuccess(new DiscordUserOrMessageAuthorEntity(userId));

        private static TypeReaderResult GetMessageAuthorResult(ulong userId, ulong messageChannelId, ulong messageId)
            => TypeReaderResult.FromSuccess(new DiscordUserOrMessageAuthorEntity(userId, messageChannelId, messageId));

        private static TypeReaderResult GetInvalidSnowflakeResult()
            => TypeReaderResult.FromError(CommandError.ParseFailed, "Snowflake was almost certainly invalid.");

        private static TypeReaderResult GetBadInputResult()
            => TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find a user or message.");
    }
}
