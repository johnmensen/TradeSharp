using System;
using System.Collections.Generic;
using System.Drawing;

namespace Entity
{
    public class BrushesStorage : IDisposable
    {
        private readonly Dictionary<Color, SolidBrush> brushes = new Dictionary<Color, SolidBrush>();

        public Brush GetBrush(Color cl)
        {
            SolidBrush brush;
            if (brushes.TryGetValue(cl, out brush)) return brush;
            brush = new SolidBrush(cl);
            brushes.Add(cl, brush);
            return brush;
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
