namespace Modix.Common.Test
{
    public class AsyncMethodWithLoggerTestContext
        : AsyncMethodTestContext
    {
        public readonly TestLoggerFactory LoggerFactory
            = new TestLoggerFactory();

        protected override void Dispose(
            bool disposeManaged)
        {
            if(disposeManaged)
                LoggerFactory.Dispose();
            base.Dispose(disposeManaged);
        }
    }
}
