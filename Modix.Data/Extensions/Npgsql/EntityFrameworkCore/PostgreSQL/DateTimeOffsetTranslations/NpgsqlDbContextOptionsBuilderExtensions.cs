using Microsoft.EntityFrameworkCore.Infrastructure;

using Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlDbContextOptionsBuilderExtensions
    {
        // TODO: Remove this if https://github.com/npgsql/efcore.pg/issues/473 ever gets resolved.
        public static NpgsqlDbContextOptionsBuilder UseDateTimeOffsetTranslations(
            this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            ((optionsBuilder as IRelationalDbContextOptionsBuilderInfrastructure)
                .OptionsBuilder as IDbContextOptionsBuilderInfrastructure)
                .AddOrUpdateExtension(new DateTimeOffsetTranslationsOptions());

            return optionsBuilder;
        }
    }
}
