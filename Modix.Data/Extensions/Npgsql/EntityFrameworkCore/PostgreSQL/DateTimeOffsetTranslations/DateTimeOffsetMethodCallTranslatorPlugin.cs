using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetMethodCallTranslatorPlugin
        : IMethodCallTranslatorPlugin
    {
        public DateTimeOffsetMethodCallTranslatorPlugin(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new[]
            {
                new DateTimeOffsetMethodCallTranslator(sqlExpressionFactory)
            };
        }

        public IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
