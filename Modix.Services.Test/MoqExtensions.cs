using System;
using System.Linq.Expressions;
using Moq;

namespace Modix.Services.Test
{
    public static class MoqExtensions
    {
        public static void ShouldHaveReceived<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression)
            where T : class
            => mock.Verify(expression);
    }
}
