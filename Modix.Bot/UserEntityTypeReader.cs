using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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
                //Any ulong is technically a valid snowflake, but we try to do some basic validation
                //by parsing the timestamp (in ms) part out of it - we consider it to be an invalid snowflake if:
                // - it's less than or equal to the discord epoch baseline
                // - it's greater than or equal to the current timestamp
                var snowflakeTimestamp = (long)(uid >> 22);
                const long discordEpochUnixTime = 1420070400000;

                //Jan 1, 2015
                var discordEpoch = DateTimeOffset.FromUnixTimeMilliseconds(discordEpochUnixTime);

                //The supposed timestamp
                var snowFlakeDateTime = DateTimeOffset.FromUnixTimeMilliseconds(snowflakeTimestamp + discordEpochUnixTime);
                if (snowFlakeDateTime <= discordEpoch || snowFlakeDateTime >= DateTimeOffset.UtcNow)
                {
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Snowflake was almost certainly invalid.");
                }

                return TypeReaderResult.FromSuccess(new DiscordUserEntity(uid));
            }

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find user / parse user ID");
        }
    }
}
