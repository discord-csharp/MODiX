using System;

using Microsoft.Extensions.DependencyInjection;

using Remora.Commands.Extensions;

namespace Modix.RemoraShim.Parsers
{
    public static class Setup
    {
        public static IServiceCollection AddParsers(this IServiceCollection services)
            => services
                .AddParser<UserOrMessageAuthor, UserOrMessageAuthorParser>()
                .AddParser<TimeSpan, TimeSpanParser>();
    }
}
