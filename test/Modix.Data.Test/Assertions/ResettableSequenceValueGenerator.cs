using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public abstract class ResettableSequenceValueGenerator<T> : ValueGenerator<T>
    {
        public abstract void SetValue([AllowNull] T value);
    }

}
