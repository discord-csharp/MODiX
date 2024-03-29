using System;
using System.Threading;

namespace Modix.Common.Test
{
    public class AsyncMethodTestContext
        : IDisposable
    {
        public readonly CancellationTokenSource CancellationTokenSource
            = new CancellationTokenSource();

        public CancellationToken CancellationToken
            => CancellationTokenSource.Token;

        ~AsyncMethodTestContext()
            => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(
            bool disposeManaged)
        {
            if(disposeManaged)
                CancellationTokenSource.Dispose();
        }
    }
}
