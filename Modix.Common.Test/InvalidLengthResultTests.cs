using System;
using System.Collections.Generic;
using System.Text;
using Modix.Common.ErrorHandling;
using NUnit.Framework;
using Shouldly;

namespace Modix.Common.Test
{
    public class InvalidLengthResultTests
    {
        //Order is Length, Minimum, Maximum
        [TestCase(5, 0, 10, ExpectedResult = true)]    // Length < Max            == success
        [TestCase(15, 10, 0, ExpectedResult = true)]   // Length > Min            == success
        [TestCase(5, 5, 0, ExpectedResult = true)]     // Length == Min           == success
        [TestCase(5, 0, 5, ExpectedResult = true)]     // Length == Max           == success
        [TestCase(5, 3, 5, ExpectedResult = true)]     // Length == Max, Min == ? == success
        [TestCase(3, 1, 5, ExpectedResult = true)]     // Max > Length > Min      == success
        [TestCase(10, 10, 10, ExpectedResult = true)]  // Length == Min == Max    == success
        [TestCase(15, 0, 10, ExpectedResult = false)]  // Length > Max            == failure
        [TestCase(5, 10, 0, ExpectedResult = false)]   // Length < Min            == failure
        [TestCase(8, 1, 5, ExpectedResult = false)]    // Length > Max > Min      == failure
        [TestCase(10, 12, 12, ExpectedResult = false)] // Length != (Min == Max)  == failure
        public bool Constructor_WithValidParams_ReturnsCorrectResult(int length, int min, int max)
        {
            var result = new InvalidLengthResult("Test", length, min, max);
            return result.IsSuccess;
        }

        [Test]
        public void InvalidLengthResult_ShouldThrow_IfMinimumLessThanZero()
        {
            Should.Throw<ArgumentException>(() => new InvalidLengthResult("test", length: 0, minimum: -1));
        }

        [Test]
        public void InvalidLengthResult_ShouldThrow_IfMaximumLessThanZero()
        {
            Should.Throw<ArgumentException>(() => new InvalidLengthResult("test", length: 0, maximum: -1));
        }
    }
}
