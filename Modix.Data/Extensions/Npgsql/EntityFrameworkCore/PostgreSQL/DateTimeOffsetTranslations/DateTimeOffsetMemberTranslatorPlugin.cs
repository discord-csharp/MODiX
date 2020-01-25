using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetMemberTranslatorPlugin
        : IMemberTranslatorPlugin
    {
        public DateTimeOffsetMemberTranslatorPlugin(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new[]
            {
                new DateTimeOffsetMemberTranslator(sqlExpressionFactory)
            };
        }

        public IEnumerable<IMemberTranslator> Translators { get; }
    }
}
