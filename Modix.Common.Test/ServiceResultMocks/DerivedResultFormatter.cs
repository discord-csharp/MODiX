using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Common.Test
{
    public class DerivedResultFormatter : StringResultFormatter<DerivedServiceResult>
    {
        public const string Value = "Derived";

        public override string Format(DerivedServiceResult result)
        {
            return Value;
        }
    }
}
