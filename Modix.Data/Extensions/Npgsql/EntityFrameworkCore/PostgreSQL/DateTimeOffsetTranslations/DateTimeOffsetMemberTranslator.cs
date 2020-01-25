using System;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetMemberTranslator
        : IMemberTranslator
    {
        public DateTimeOffsetMemberTranslator(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression? Translate(
                SqlExpression instance,
                MemberInfo member,
                Type returnType)
            => ((member.DeclaringType == typeof(DateTimeOffset))
                    && (member.Name == nameof(DateTimeOffset.Date)))
                ? _sqlExpressionFactory.Function("DATE_TRUNC", new[] { _sqlExpressionFactory.Constant("day"), instance }, returnType)
                : null;

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
    }
}
