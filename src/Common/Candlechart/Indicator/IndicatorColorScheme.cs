using System.Drawing;

namespace Candlechart.Indicator
{
    public static class IndicatorColorScheme
    {
        public static readonly Color[] colorsFill = new[] { Color.Moccasin, 
            Color.PaleGreen, Color.Plum, Color.Gold, Color.SkyBlue, Color.MistyRose };
        public static readonly Color[] colorsLine = new[] { Color.DarkSalmon, 
            Color.DarkGreen, Color.DarkMagenta, Color.DarkGoldenrod, Color.SteelBlue, Color.Maroon };

        public static readonly Color[] colorsOfBlue = new[] { Color.Navy, 
            Color.GreenYellow, Color.LightSeaGreen, Color.DodgerBlue, Color.Green, Color.MediumAquamarine };
        public static readonly Color[] colorsOfRed = new[] { Color.Maroon, 
            Color.Orange, Color.Fuchsia, Color.Tomato, Color.Red, Color.Pink };
        
        public const int PresetColorsCount = 6;
    }
}
