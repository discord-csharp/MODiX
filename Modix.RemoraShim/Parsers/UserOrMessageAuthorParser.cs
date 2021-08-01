using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Core;
using Remora.Results;

namespace Modix.RemoraShim.Parsers
{
    public record UserOrMessageAuthor(IUser User)
    {
        public UserOrMessageAuthor(IUser user, Snowflake channelId, Snowflake messageId) : this(user)
        {
            ChannelId = channelId;
            MessageId = messageId;
        }

        public Snowflake? ChannelId { get; init; }
        public Snowflake? MessageId { get; init; }
    }

    internal class UserOrMessageAuthorParser : AbstractTypeParser<UserOrMessageAuthor>
    {
        public UserOrMessageAuthorParser(ICommandContext context, IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi, IDiscordRestUserAPI userApi)
        {
            _context = context;
            _channelApi = channelApi;
            _guildApi = guildApi;
            _userApi = userApi;
        }

        public override async ValueTask<Result<UserOrMessageAuthor>> TryParse(string value, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            value = value.Trim(',', '.');

            // value could be:
            // - user ID
            // - user mention
            // - message ID

            if (Snowflake.TryParse(value, out var nullableSnowflake) || Snowflake.TryParse(value.Unmention(), out nullableSnowflake))
            {
                var snowflake = nullableSnowflake.GetValueOrDefault();

                var userResult = await _userApi.GetUserAsync(snowflake, ct);

                if (userResult.IsSuccess)
                    return new UserOrMessageAuthor(userResult.Entity);

                var currentChannelMessageResult = await _channelApi.GetChannelMessageAsync(_context.ChannelID, snowflake, ct);
                if (currentChannelMessageResult.IsSuccess)
                    return new UserOrMessageAuthor(currentChannelMessageResult.Entity.Author, currentChannelMessageResult.Entity.ChannelID, currentChannelMessageResult.Entity.ID);

                if (!_context.GuildID.HasValue)
                    return FromError(value);

                var currentGuildChannelsResult = await _guildApi.GetGuildChannelsAsync(_context.GuildID.Value, ct);

                if (!currentGuildChannelsResult.IsSuccess)
                    return FromError(currentGuildChannelsResult);

                foreach (var channel in currentGuildChannelsResult.Entity.Where(x => x.ID != _context.ChannelID))
                {
                    var messageResult = await _channelApi.GetChannelMessageAsync(channel.ID, snowflake, ct);

                    if (messageResult.IsSuccess)
                        return new UserOrMessageAuthor(messageResult.Entity.Author, channel.ID, snowflake);
                }
            }

            return FromError(value);
        }

        private static Result<UserOrMessageAuthor> FromError<T>(Result<T> result)
            => Result<UserOrMessageAuthor>.FromError(result);

        private static Result<UserOrMessageAuthor> FromError(string value, string reason = "Could not find a matching user or message.")
            => Result<UserOrMessageAuthor>.FromError(new ParsingError<UserOrMessageAuthor>(value, reason));

        private readonly ICommandContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestUserAPI _userApi;
    }
}
