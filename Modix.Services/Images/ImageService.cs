using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.Caching.Memory;

using Modix.Services.Images.ColorQuantization;
using Modix.Services.Utilities;

using Serilog;
using SixLabors.ImageSharp.PixelFormats;

namespace Modix.Services.Images
{
    public interface IImageService
    {
        ValueTask<Color> GetDominantColorAsync(Uri location);
    }

    internal sealed class ImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public ImageService(IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = memoryCache;
        }

        public async ValueTask<Color> GetDominantColorAsync(Uri location)
        {
            try
            {
                var key = GetKey(location);

                if (!_cache.TryGetValue(key, out Color color))
                {
                    var imageBytes = await _httpClientFactory.CreateClient(HttpClientNames.Timeout300ms).GetByteArrayAsync(location);
                    color = GetDominantColor(imageBytes.AsSpan());

                    _cache.Set(key, color, TimeSpan.FromDays(7));
                }

                return color;
            }
            catch (Exception ex)
            {
                Log.Information(ex, "An error occurred while attempting to determine the dominant color of an image.");
                return Color.Default;
            }
        }

        private static Color GetDominantColor(ReadOnlySpan<byte> imageBytes)
        {
            var colorTree = new Octree();

            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);

            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    var pixel = img[x, y];

                    // Don't include transparent pixels.
                    if (pixel.A > 127)
                    {
                        var color = System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);

                        colorTree.Add(color);
                    }
                }
            }

            for (var i = 0; i < 7; i++)
            {
                colorTree.Reduce();
            }

            var mostCommonPaletteColor = colorTree.GetPalette().OrderByDescending(x => x.Weight * x.Color.GetSaturation()).FirstOrDefault().Color;

            // TODO Investigate why we cannot cast here anymore (return (Color)mostCommonPaletteColor;)
            return new Color((uint)mostCommonPaletteColor.ToArgb() << 8 >> 8);
        }

        private object GetKey(Uri uri) => new { Target = "DominantColor", uri.AbsoluteUri };
    }
}
