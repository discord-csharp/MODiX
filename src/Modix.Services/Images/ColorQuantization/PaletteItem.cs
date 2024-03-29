using System.Drawing;

namespace Modix.Services.Images.ColorQuantization
{
    public struct PaletteItem
    {
        public Color Color { get; set; }

        public int Weight { get; set; }
    }
}
