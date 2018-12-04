using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;

namespace Modix.Common.Test
{
    public class DerivedServiceResult : ServiceResult
    {
        public const string Value = "[DERIVED_SERVICE_RESULT]";

        public override string ToString()
        {
            return Value;
        }
    }
}
