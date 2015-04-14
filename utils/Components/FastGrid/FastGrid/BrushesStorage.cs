using System;
using System.Collections.Generic;
using System.Drawing;

namespace FastGrid
{
    /// <summary>
    /// dictionary that stores solid brushes
    /// should be used in a graphics object lifetime
    /// </summary>
    public class BrushesStorage : IDisposable
    {
        private readonly Dictionary<Color, SolidBrush> brushes = new Dictionary<Color, SolidBrush>();

        public SolidBrush GetBrush(Color cl)
        {
            SolidBrush brush;
            if (brushes.TryGetValue(cl, out brush)) return brush;
            brush = new SolidBrush(cl);
            brushes.Add(cl, brush);
            return brush;
        }

        public void SetAlpha(byte alpha)
        {
            foreach (var brush in brushes)
            {
                brush.Value.Color = Color.FromArgb(alpha, brush.Value.Color);
            }
        }

        public void Dispose()
        {
            foreach (var b in brushes)
            {
                b.Value.Dispose();
            }
        }
    }
}
