using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetMemberTranslator
        : IMemberTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public DateTimeOffsetMemberTranslator(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression? Translate(SqlExpression instance, MemberInfo member, Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (member.DeclaringType == typeof(DateTimeOffset) && member.Name == nameof(DateTimeOffset.Date))
            {
                return _sqlExpressionFactory.Function("DATE_TRUNC",
                    new[] {_sqlExpressionFactory.Constant("day"), instance},
                    false,
                    new[] {true},
                    returnType);
            }

            return null;
        }
    }
}
