#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Discord;

namespace Modix.Services.Core
{
    public interface ICommandPrefixParser
    {
        Task<int?> TryFindCommandArgPosAsync(
            IUserMessage message,
            CancellationToken cancellationToken);
    }

}
