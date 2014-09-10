using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
//Allows to get series colors from PREDEFINED by Microsoft color palette
namespace L3.Cargo.DetectorPlot
{
    //=============== color class ===============
    static class GetColors
    {
        public static System.Drawing.Color[] GetPaletteColors(this ChartColorPalette value)
        {
            switch (value)
            {
                case ChartColorPalette.Berry:
                    return GetPColors(0x8a2be2, 0xba55d3, 0x4169e1, 0xc71585, 0x0000ff, 0x8a2be2, 0xda70d6, 0x7b68ee, 0xc000c0, 0x0000cd, 0x800080);
                case ChartColorPalette.Bright:
                    return GetPColors(0x008000, 0x0000ff, 0x800080, 0x00ff00, 0xff00ff, 0x008080, 0xffff00, 0x808080, 0x00ffff, 0x000080, 0x800000, 0xff0000, 0x808000, 0xc0c0c0, 0xff6347, 0xffe4b5);
                case ChartColorPalette.BrightPastel:
                    return GetPColors(0x418cf0, 0xfcb441, 0xe0400a, 0x056492, 0xbfbfbf, 0x1a3b69, 0xffe382, 0x129cdd, 0xca6b4b, 0x005cdb, 0xf3d288, 0x506381, 0xf1b9a8, 0xe0830a, 0x7893be);
                case ChartColorPalette.Chocolate:
                    return GetPColors(0xa0522d, 0xd2691e, 0x8b0000, 0xcd853f, 0xa52a2a, 0xf4a460, 0x8b4513, 0xc04000, 0xb22222, 0xb65c3a);
                case ChartColorPalette.EarthTones:
                    return GetPColors(0xff8000, 0xb8860b, 0xc04000, 0x6b8e23, 0xcd853f, 0xc0c000, 0x228b22, 0xd2691e, 0x808000, 0x20b2aa, 0xf4a460, 0x00c000, 0x8fbc8b, 0xb22222, 0x8b4513, 0xc00000);
                case ChartColorPalette.Excel:
                    return GetPColors(0x9999ff, 0x993366, 0xffffcc, 0xccffff, 0x660066, 0xff8080, 0x0066cc, 0xccccff, 0x000080, 0xff00ff, 0xffff00, 0x00ffff, 0x800080, 0x800000, 0x008080, 0x0000ff);
                case ChartColorPalette.Fire:
                    return GetPColors(0xffd700, 0xff0000, 0xff1493, 0xdc143c, 0xff8c00, 0xff00ff, 0xffff00, 0xff4500, 0xc71585, 0xdde221);
                case ChartColorPalette.Grayscale:
                    return GetPColors(0xc8c8c8, 0xbdbdbd, 0xb2b2b2, 0xa7a7a7, 0x9c9c9c, 0x919191, 0x868686, 0x7b7b7b, 0x707070, 0x656565, 0x5a5a5a, 0x4f4f4f, 0x444444, 0x393939, 0x2e2e2e, 0x232323);
                case ChartColorPalette.Light:
                    return GetPColors(0xe6e6fa, 0xfff0f5, 0xffdab9, 0xfffacd, 0xffe4e1, 0xf0fff0, 0xf0f8ff, 0xf5f5f5, 0xfaebd7, 0xe0ffff);
                case ChartColorPalette.Pastel:
                    return GetPColors(0x87ceeb, 0x32cd32, 0xba55d3, 0xf08080, 0x4682b4, 0x9acd32, 0x40e0d0, 0xff69b4, 0xf0e68c, 0xd2b48c, 0x8fbc8b, 0x6495ed, 0xdda0dd, 0x5f9ea0, 0xffdab9, 0xffa07a);
                case ChartColorPalette.SeaGreen:
                    return GetPColors(0x2e8b57, 0x66cdaa, 0x4682b4, 0x008b8b, 0x5f9ea0, 0x3cb371, 0x48d1cc, 0xb0c4de, 0xffffff, 0x87ceeb);
                case ChartColorPalette.SemiTransparent:
                    return GetPColors(0xff6969, 0x69ff69, 0x6969ff, 0xffff69, 0x69ffff, 0xff69ff, 0xcdb075, 0xffafaf, 0xafffaf, 0xafafff, 0xffffaf, 0xafffff, 0xffafff, 0xe4d5b5, 0xa4b086, 0x819ec1);
                case ChartColorPalette.None:
                default:
                    return GetPColors(0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000);
            }

        }

        private static System.Drawing.Color[] GetPColors(params Int32[] values)
        {
            System.Drawing.Color MyColor = System.Drawing.Color.FromArgb(0xFF0000);
            return values.Select(value => System.Drawing.Color.FromArgb(value)).ToArray(); // alpha channel of 255 for fully opaque
        }
    }
}

