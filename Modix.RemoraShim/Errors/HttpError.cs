using System.Net;

using Remora.Results;

namespace Modix.RemoraShim.Errors
{
    internal record HttpError(HttpStatusCode HttpStatusCode)
        : ResultError($"HTTP operation failed with status code {(int)HttpStatusCode} {HttpStatusCode}");
}
