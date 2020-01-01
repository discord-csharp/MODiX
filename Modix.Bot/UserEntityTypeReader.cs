using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Utilities;

namespace Modix
{
    public class DiscordUserEntity : IEntity<ulong>
    {
        public ulong Id { get; }
        public DiscordUserEntity(ulong id) { Id = id; }

        public static DiscordUserEntity FromIUser(IUser user) => new DiscordUserEntity(user.Id);
    }

    public class UserEntityTypeReader : UserTypeReader<IGuildUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var baseResult = await base.ReadAsync(context, input, services);

            if (baseResult.IsSuccess)
            {
                return TypeReaderResult.FromSuccess(DiscordUserEntity.FromIUser(baseResult.BestMatch as IUser));
            }

            if (ulong.TryParse(input, out var uid) || MentionUtils.TryParseUser(input, out uid))
            {
                if (!SnowflakeUtilities.IsValidSnowflake(uid))
                {
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Snowflake was almost certainly invalid.");
                }

                return TypeReaderResult.FromSuccess(new DiscordUserEntity(uid));
            }

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find user / parse user ID");
        }
    }
}
