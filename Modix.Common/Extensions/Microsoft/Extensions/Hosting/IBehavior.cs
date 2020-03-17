using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public interface IBehavior
    {
        Task StartAsync(
            CancellationToken cancellationToken);

        Task StopAsync(
            CancellationToken cancellationToken);
    }
}
