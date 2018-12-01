using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;
using NUnit.Framework;
using Shouldly;

namespace Modix.Common.Test
{
    public class ServiceResultTests
    {
        [Test]
        public void SuccessfulResult_IsNotFailure()
        {
            var result = new MockServiceResult { OverrideSuccess = true };
            result.IsFailure.ShouldBe(false);
        }

        [Test]
        public void NotSuccessfulResult_IsFailure()
        {
            var result = new MockServiceResult { OverrideSuccess = false };
            result.IsFailure.ShouldBe(true);
        }

        [Test]
        public void FromResult_IsSuccess_IfArgumentNotSet()
        {
            var result = ServiceResult.FromResult("");
            result.IsSuccess.ShouldBe(true);
        }

        [Test]
        public void FromResult_IsFailure_IfArgumentSet()
        {
            var result = ServiceResult.FromResult("", false);
            result.IsFailure.ShouldBe(true);
        }
    }
}
