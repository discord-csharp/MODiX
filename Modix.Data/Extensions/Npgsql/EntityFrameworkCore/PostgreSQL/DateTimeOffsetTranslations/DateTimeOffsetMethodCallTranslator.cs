using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetMethodCallTranslator
        : IMethodCallTranslator
    {
        public DateTimeOffsetMethodCallTranslator(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        #pragma warning disable EF1001 // Internal EF Core API usage.
        public SqlExpression? Translate(
                SqlExpression instance,
                MethodInfo method,
                IReadOnlyList<SqlExpression> arguments)
            => ((method.DeclaringType == typeof(DateTimeOffset))
                    && (method.Name == nameof(DateTimeOffset.ToUniversalTime))
                    && _sqlExpressionFactory is NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
                ? npgsqlSqlExpressionFactory.AtTimeZone(
                    instance,
                    npgsqlSqlExpressionFactory.Constant("UTC"),
                    method.ReturnType)
                : null;
        #pragma warning restore EF1001 // Internal EF Core API usage.

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
    }
}
