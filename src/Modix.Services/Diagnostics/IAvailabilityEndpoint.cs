using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Diagnostics
{
    public interface IAvailabilityEndpoint
        : IDiagnosticEndpoint
    {
        Task<bool> GetAvailabilityAsync(
            CancellationToken cancellationToken);
    }
}
