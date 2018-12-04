using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;
using Shouldly;

namespace Modix.Common.Test
{
    public static class ServiceResultTestExtensions
    {
        public static void ShouldBeSuccessful(this ServiceResult result)
        {
            result.IsSuccess.ShouldBeTrue();
        }

        public static void ShouldBeFailure(this ServiceResult result)
        {
            result.IsFailure.ShouldBeTrue();
        }
    }
}
