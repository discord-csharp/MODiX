using System.Linq.Expressions;

namespace Shouldly
{
    public static class ExpressionAssertions
    {
        public static void ShouldBe(this Expression actual, Expression expected)
            => actual?.ToString().ShouldBe(expected?.ToString());
    }
}
