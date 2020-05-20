#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using Modix.Services.Core;

namespace Modix.Bot.Behaviors
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class CommandPrefixParser
        : ICommandPrefixParser
    {
        public CommandPrefixParser(
            ISelfUserProvider selfUserProvider)
        {
            _selfUserProvider = selfUserProvider;
        }

        public async Task<int?> TryFindCommandArgPosAsync(
            IUserMessage message,
            CancellationToken cancellationToken)
        {
            // Optimization: Odds are this will only ever be called on one message, per service scope,
            // so we can cache the result.
            if (_lastResult.HasValue
                    && _lastResult.Value.message.Id == message.Id)
                return _lastResult.Value.argPos;

            var argPos = default(int);

            _lastResult = (
                message,
                (message.HasCharPrefix('!', ref argPos)
                        || message.HasMentionPrefix(await _selfUserProvider.GetSelfUserAsync(cancellationToken), ref argPos))
                    ? argPos
                    : null as int?);

            return _lastResult.Value.argPos;
        }

        private readonly ISelfUserProvider _selfUserProvider;

        private (IUserMessage message, int? argPos)? _lastResult;
    }
}
