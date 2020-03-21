using System;
using System.Linq.Expressions;

using Moq;

namespace Shouldly
{
    public static class MoqAssertions
    {
        public static void ShouldHaveReceived<T>(this Mock<T> mock, Expression<Action<T>> expression, Times? times = null) where T : class
            => mock.Verify(expression, times ?? Times.AtLeastOnce());

        public static void ShouldHaveReceived<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression, Times? times = null) where T : class
            => mock.Verify(expression, times ?? Times.AtLeastOnce());

        public static void ShouldNotHaveReceived<T>(this Mock<T> mock, Expression<Action<T>> expression) where T : class
            => mock.Verify(expression, Times.Never);

        public static void ShouldNotHaveReceived<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression) where T : class
            => mock.Verify(expression, Times.Never);
    }
}
