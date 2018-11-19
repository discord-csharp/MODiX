using System.Threading;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class ResettableInt64SequenceValueGenerator : ResettableSequenceValueGenerator<long>
    {
        public override bool GeneratesTemporaryValues
            => false;

        public override long Next(EntityEntry entry)
            => Interlocked.Increment(ref _currentValue);

        public override void SetValue(long value)
            => _currentValue = value;

        private long _currentValue
            = 0;
    }
}
