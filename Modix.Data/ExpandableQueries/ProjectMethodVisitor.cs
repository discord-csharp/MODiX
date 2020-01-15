using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Modix.Data.ExpandableQueries
{
    public class ProjectMethodVisitor : ExpressionVisitor
    {
        static ProjectMethodVisitor()
        {
            _projectMethod = typeof(ExpandableExtensions)
                .GetMethod(nameof(ExpandableExtensions.Project))!;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node.Method.IsGenericMethod) && (node.Method.GetGenericMethodDefinition() == _projectMethod))
            {
                var input = node.Arguments[0];
                var projection = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;

                var parameter = projection.Parameters.First();

                _parameterReplacements.TryGetValue(parameter, out var oldReplacement);
                _parameterReplacements[parameter] = input;

                var visitedExpression = Visit(projection.Body);

                if (oldReplacement is null)
                    _parameterReplacements.Remove(parameter);
                else
                    _parameterReplacements[parameter] = oldReplacement;

                return visitedExpression;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => _parameterReplacements.TryGetValue(node, out var replacement)
                ? Visit(replacement)
                : base.VisitParameter(node);

        private readonly Dictionary<ParameterExpression, Expression> _parameterReplacements
            = new Dictionary<ParameterExpression, Expression>();

        private static readonly MethodInfo _projectMethod;
    }
}
