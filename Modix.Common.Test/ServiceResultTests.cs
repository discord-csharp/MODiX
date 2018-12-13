using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void ConditionalServiceResult_IsSuccessful_IfConditionSuccessful()
        {
            var result = new ConditionalServiceResult<string, ServiceResult<string>>(
                ServiceResult.FromResult("test"), ServiceResult.FromSuccess());

            result.ShouldBeSuccessful();
        }

        [Test]
        public void ConditionalServiceResult_IsFailure_IfConditionSuccessful()
        {
            var result = new ConditionalServiceResult<string, ServiceResult<string>>(
                ServiceResult.FromResult("test"), ServiceResult.FromError("Failure"));

            result.ShouldBeFailure();
        }

        [Test]
        public void ConditionalServiceResult_HasResult_IfProvided()
        {
            var result = new ConditionalServiceResult<string, ServiceResult<string>>(
                ServiceResult.FromResult("test"), ServiceResult.FromSuccess());

            result.Result.ShouldBe("test");
        }

        [Test]
        public async Task ShortCircuit_DoesNotAwaitTask_IfFailure()
        {
            bool shouldBeFalse = false;

            var task = new Task<object>(() => shouldBeFalse = true);
            var result = await ServiceResult.FromError("Failure").ShortCircuitAsync(task);

            shouldBeFalse.ShouldBeFalse();
        }

        [Test]
        public async Task ShortCircuit_AwaitsTask_IfSuccess()
        {
            bool shouldBeTrue = false;

            var task = new Task<object>(() => shouldBeTrue = true);
            task.Start();

            var result = await ServiceResult.FromSuccess().ShortCircuitAsync(task);

            shouldBeTrue.ShouldBeTrue();
        }
    }
}
