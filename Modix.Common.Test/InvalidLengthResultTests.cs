using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;
using NUnit.Framework;

namespace Modix.Common.Test
{
    public class InvalidLengthResultTests
    {
        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthLessThanMax()
        {
            var result = new InvalidLengthResult("test", length: 5, maximum: 10);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsFailure_IfLengthGreaterThanMax()
        {
            var result = new InvalidLengthResult("test", length: 15, maximum: 10);
            result.ShouldBeFailure();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthGreaterThanMin()
        {
            var result = new InvalidLengthResult("test", length: 15, minimum: 10);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsFailure_IfLengthLessThanMin()
        {
            var result = new InvalidLengthResult("test", length: 5, minimum: 10);
            result.ShouldBeFailure();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthEqualToMin()
        {
            var result = new InvalidLengthResult("test", length: 5, minimum: 5);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthEqualToMax()
        {
            var result = new InvalidLengthResult("test", length: 5, maximum: 5);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthEqualToMax_WithMinSpecified()
        {
            var result = new InvalidLengthResult("test", length: 5, maximum: 5, minimum: 3);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthEqualToMin_WithMaxSpecified()
        {
            var result = new InvalidLengthResult("test", length: 3, maximum: 5, minimum: 3);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthBetweenMinAndMax()
        {
            var result = new InvalidLengthResult("test", length: 3, maximum: 5, minimum: 1);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsFailure_IfLengthNotBetweenMinAndMax()
        {
            var result = new InvalidLengthResult("test", length: 8, maximum: 5, minimum: 1);
            result.ShouldBeFailure();
        }

        [Test]
        public void InvalidLengthResult_IsSuccess_IfLengthEqualToMinAndMax()
        {
            var result = new InvalidLengthResult("test", length: 10, maximum: 10, minimum: 10);
            result.ShouldBeSuccessful();
        }

        [Test]
        public void InvalidLengthResult_IsFailure_IfLengthNotEqualToMinAndMax()
        {
            var result = new InvalidLengthResult("test", length: 10, maximum: 12, minimum: 12);
            result.ShouldBeFailure();
        }
    }
}
