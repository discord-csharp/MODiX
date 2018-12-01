using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;

namespace Modix.Common.Test
{
    public class MockServiceResult : ServiceResult
    {
        public bool OverrideSuccess
        {
            get => IsSuccess;
            set => IsSuccess = value;
        }
    }
}
