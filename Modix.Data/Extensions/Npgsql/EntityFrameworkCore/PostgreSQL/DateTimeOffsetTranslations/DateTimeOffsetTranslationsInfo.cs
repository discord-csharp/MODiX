using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetTranslationsInfo
        : DbContextOptionsExtensionInfo
    {
        public DateTimeOffsetTranslationsInfo(
                IDbContextOptionsExtension extension)
            : base(
                extension) { }

        public override bool IsDatabaseProvider
            => false;

        public override string LogFragment { get; }
            = "using DateTimeOffset translation extension";

        public override long GetServiceProviderHashCode()
            => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}
