using Microsoft.Extensions.DependencyInjection;

namespace System
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }

    [ServiceBinding(ServiceLifetime.Singleton)]
    public class SystemClock
        : ISystemClock
    {
        public DateTimeOffset UtcNow
            => DateTimeOffset.UtcNow;
    }
}
