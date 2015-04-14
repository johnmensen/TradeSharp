using System;
using System.Collections.Generic;
using System.Drawing;

namespace Entity
{
    public class FontStorage : IDisposable
    {
        private readonly Dictionary<FontStyle, Font> fonts = new Dictionary<FontStyle, Font>();

        private readonly Font speciman;

        public FontStorage(Font defaultFont)
        {
            speciman = defaultFont;
            
        }

        public Font GetFont(FontStyle style)
        {
            Font font;
            if (fonts.TryGetValue(style, out font)) return font;

            var newFont = new Font(speciman, style);
            fonts.Add(style, newFont);
            return newFont;
        }

        public void Dispose()
        {
            foreach (var f in fonts)
                f.Value.Dispose();
            fonts.Clear();
        }
    }
}
