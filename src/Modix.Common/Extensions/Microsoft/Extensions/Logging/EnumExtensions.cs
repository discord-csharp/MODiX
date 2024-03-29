using System;

namespace Microsoft.Extensions.Logging
{
    public static class EnumExtensions
    {
        public static EventId ToEventId<TEnum>(
                    this TEnum @event)
                where TEnum : struct, IConvertible
            => new EventId(
                @event.ToInt32(null),
                @event.ToString());
    }
}
