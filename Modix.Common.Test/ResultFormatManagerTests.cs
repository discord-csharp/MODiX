using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Modix.Common.ErrorHandling;
using NUnit.Framework;
using Shouldly;

namespace Modix.Common.Test
{
    [TestFixture]
    public class ResultFormatManagerTests
    {
        private ResultFormatManager ResultFormatManager;

        [OneTimeSetUp]
        public void SetUp()
        {
            var services = new ServiceCollection()
                .AddSingleton<IResultFormatter<ServiceResult, string>, StringResultFormatter<ServiceResult>>()
                .AddSingleton<IResultFormatter<DerivedServiceResult, string>, DerivedResultFormatter>()
                .BuildServiceProvider();

            ResultFormatManager = new ResultFormatManager(services);
        }

        [Test]
        public void ResultFormatManager_ReturnsDefaultFormatter_IfNoneFound()
        {
            var unknownResult = new UnknownServiceResult();
            var result = ResultFormatManager.Format<UnknownServiceResult, string>(unknownResult);

            result.ShouldStartWith(StringResultFormatter<ServiceResult>.Value);
        }

        [Test]
        public void ResultFormatManager_ReturnsDefaultFormatter_ForBaseServiceResult()
        {
            var serviceResult = ServiceResult.FromSuccess();
            var result = ResultFormatManager.Format<ServiceResult, string>(serviceResult);

            result.ShouldStartWith(StringResultFormatter<ServiceResult>.Value);
        }

        [Test]
        public void ResultFormatManager_ReturnsSpecializedFormatter_ForDerivedType()
        {
            var derivedResult = new DerivedServiceResult();
            var result = ResultFormatManager.Format<ServiceResult, string>(derivedResult);

            result.ShouldStartWith(DerivedResultFormatter.Value);
        }

        [Test]
        public void ResultFormatManager_ReturnsSpecializedFormatter_ForFailedConditionalResult()
        {
            var derivedResult = new DerivedServiceResult(false);
            var conditional = new ConditionalServiceResult<string, ServiceResult<string>>
                (ServiceResult.FromResult(""), derivedResult);

            var result = ResultFormatManager.Format<ServiceResult, string>(conditional);

            result.ShouldStartWith(DerivedResultFormatter.Value);
        }
    }
}
