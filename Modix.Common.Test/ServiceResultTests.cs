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
        public void ServiceResult_SuccessfulResult_IsNotFailure()
        {
            var result = new MockServiceResult { OverrideSuccess = true };

            //Make sure it's successful by explicitly checking IsFailure,
            //as we are testing its implementation
            result.IsFailure.ShouldBeFalse(); 
        }

        [Test]
        public void ServiceResult_NotSuccessfulResult_IsFailure()
        {
            var result = new MockServiceResult { OverrideSuccess = false };
            result.ShouldBeFailure();
        }

        [Test]
        public void ServiceResult_FromResult_IsSuccess_IfArgumentNotSet()
        {
            var result = ServiceResult.FromResult("");
            result.ShouldBeSuccessful();
        }

        [Test]
        public void ServiceResult_FromResult_IsFailure_IfArgumentSet()
        {
            var result = ServiceResult.FromResult("", false);
            result.ShouldBeFailure();
        }
    }
}
