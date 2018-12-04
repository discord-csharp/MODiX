using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;

namespace Modix.Common.Test
{
    public class StringResultFormatter<T> : IResultFormatter<T, string> where T : ServiceResult
    {
        public const string Value = "Base";

        public virtual string Format(T result)
        {
            return Value;
        }
    }
}
