using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Diagnostics
{
    public interface ILatencyEndpoint
        : IDiagnosticEndpoint
    {
        Task<long?> GetLatencyAsync(
            CancellationToken cancellationToken);
    }
}
