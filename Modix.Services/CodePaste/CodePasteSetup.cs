using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.CodePaste
{
    public static class CodePasteSetup
    {
        public static IServiceCollection AddCodePaste(this IServiceCollection services)
            => services
                .AddSingleton<CodePasteService>();
    }
}
