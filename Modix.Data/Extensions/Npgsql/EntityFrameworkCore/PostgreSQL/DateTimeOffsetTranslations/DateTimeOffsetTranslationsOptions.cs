using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.DateTimeOffsetTranslations
{
    public class DateTimeOffsetTranslationsOptions
        : IDbContextOptionsExtension
    {
        public DateTimeOffsetTranslationsOptions()
        {
            Info = new DateTimeOffsetTranslationsInfo(this);
        }

        public DbContextOptionsExtensionInfo Info { get; }

        public void ApplyServices(IServiceCollection services)
            => services
                .AddSingleton<IMemberTranslatorPlugin, DateTimeOffsetMemberTranslatorPlugin>()
                .AddSingleton<IMethodCallTranslatorPlugin, DateTimeOffsetMethodCallTranslatorPlugin>();

        public void Validate(IDbContextOptions options) { }
    }
}
