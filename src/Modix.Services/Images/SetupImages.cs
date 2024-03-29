using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Images
{
    public static class ImagesSetup
    {
        public static IServiceCollection AddImages(this IServiceCollection services)
            => services
                .AddScoped<IImageService, ImageService>();
    }
}
