using System;
using System.Collections.Generic;
using Discord;

namespace Modix.Services.Utilities
{
    public class ColorUtils
    {
        /// <summary>
        /// Returns rainbow table of <see cref="colorCount"/> colors
        /// </summary>
        public static List<Color> GetRainbowColors(int colorCount)
        {
            var ret = new List<Color>(colorCount);
            var p = 360f / colorCount;

            for (var n = 0; n < colorCount; n++)
            {
                ret.Add(HsvToRgb(n * p, 1f, 1f));
            }

            return ret;
        }

        /// <summary>
        /// HSV -> RGB color
        /// </summary>
        public static Color HsvToRgb(float h, float s, float v)
        {
            var hi = (int) Math.Floor(h / 60.0) % 6;
            var f = (h / 60f) - MathF.Floor(h / 60f);

            var p = v * (1f - s);
            var q = v * (1f - (f * s));
            var t = v * (1f - ((1f - f) * s));

            var color = hi switch
            {
                0 => new Color(v, t, p),
                1 => new Color(q, v, p),
                2 => new Color(p, v, t),
                3 => new Color(p, q, v),
                4 => new Color(t, p, v),
                5 => new Color(v, p, q),
                _ => new Color(0x00, 0x00, 0x00)
            };
            return color;
        }
    }
}
