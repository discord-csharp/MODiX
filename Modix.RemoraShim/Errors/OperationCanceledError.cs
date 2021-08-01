using Remora.Results;

namespace Modix.RemoraShim.Errors
{
    internal record OperationCanceledError()
        : ResultError("The current operation has been canceled.");
}
