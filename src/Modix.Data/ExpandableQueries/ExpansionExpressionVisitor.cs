using System.Linq.Expressions;
using System.Reflection;

namespace Modix.Data.ExpandableQueries
{
    public class ExpansionExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
            => IsExpansionExpression(node)
                ? Visit(GetExpansionExpression(node))
                : base.VisitMember(node);

        private static bool IsExpansionExpression(MemberExpression node)
            => (node.Member is FieldInfo fieldInfo)
                && fieldInfo.IsStatic
                && !(fieldInfo.GetCustomAttribute<ExpansionExpressionAttribute>() is null);

        private static Expression GetExpansionExpression(MemberExpression node)
            => (Expression)((FieldInfo)node.Member).GetValue(null)!;
    }
}
