using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Modix.Data.Projectables
{
    public class ProjectableVisitor : ExpressionVisitor
    {
        static ProjectableVisitor()
        {
            _projectMethod = typeof(ProjectableExtensions)
                .GetMethod(nameof(ProjectableExtensions.Project));
        }

        public ProjectableVisitor(IQueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node.Method.IsGenericMethod) && (node.Method.GetGenericMethodDefinition() == _projectMethod))
            {
                var input = node.Arguments[0];
                var projection = GetProjection(node.Arguments[1]);

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

        private static LambdaExpression GetProjection(Expression projectionExpression)
        {
            if ((projectionExpression is MemberExpression memberExpression)
                && (memberExpression.Member is FieldInfo field)
                && field.IsStatic)
                return field.GetValue(null) as LambdaExpression;

            throw new InvalidOperationException($"Unable to evaluate expression \"{projectionExpression.ToString()}\" to retrieve a projection expression");
        }

        private readonly IQueryProvider _provider;

        private readonly Dictionary<ParameterExpression, Expression> _parameterReplacements
            = new Dictionary<ParameterExpression, Expression>();

        private static readonly MethodInfo _projectMethod;
    }
}
