using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix
{
    internal class UserEntity : IEntity<ulong>
    {
        public ulong Id { get; private set; }
        public UserEntity(ulong id) { Id = id; }

        public static UserEntity FromIUser(IUser user) => new UserEntity(user.Id);
    }

    public class UserEntityTypeReader : UserTypeReader<IGuildUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var baseResult = await base.ReadAsync(context, input, services);

            if (baseResult.IsSuccess)
            {
                return TypeReaderResult.FromSuccess(UserEntity.FromIUser(baseResult.BestMatch as IUser));
            }

            if (ulong.TryParse(input, out var uid))
            {
                //Any ulong is technically a valid snowflake, but we try to do some basic validation
                //by parsing the timestamp (in ms) part out of it - if it's less than or equal to 0, it's
                //before the Discord epoch of Jan 1, 2015, and thus invalid
                var snowflakeTimestamp = (long)(uid >> 22);

                if (snowflakeTimestamp <= 0)
                {
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Snowflake was almost certainly invalid.");
                }

                return TypeReaderResult.FromSuccess(new UserEntity(uid));
            }

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find user / parse user ID");
        }
    }
}
