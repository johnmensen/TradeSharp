using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Entity
{
    /// <summary>
    /// хранит объекты класса Pen, сортируя их по стилю, цвету и толщине
    /// экземпляр создается на время жизни объекта Graphics
    /// </summary>
    public class PenStorage : IDisposable
    {
        struct PenDescriber
        {
            private float width;
            private Color color;
            private DashStyle style;

            public PenDescriber(float width, Color color, DashStyle style)
            {
                this.width = width;
                this.color = color;
                this.style = style;
            }
        }

        private static readonly float[] defaultDashPattern = new[] { 4f, 3f };
        private static readonly float[] defaultDotPattern = new[] { 2f, 2f };
        private static readonly float[] defaultDashDotPattern = new[] { 4f, 3f, 1f, 3f };
        private static readonly float[] defaultDashDotDotPattern = new[] { 4f, 3f, 1f, 3f, 1f, 3f };

        private readonly Dictionary<PenDescriber, Pen> pens = new Dictionary<PenDescriber, Pen>();

        public Pen GetPen(Color color, float width = 1, DashStyle style = DashStyle.Solid)
        {
            var desc = new PenDescriber(width, color, style);
            Pen pen;
            if (pens.TryGetValue(desc, out pen)) return pen;
            pen = new Pen(color);
            if (width != 1) pen.Width = width;
            if (style != DashStyle.Solid)
            {
                pen.DashStyle = style;
                switch (pen.DashStyle)
                {
                    case DashStyle.Dot:
                        pen.DashPattern = defaultDotPattern;
                        break;
                    case DashStyle.Dash:
                        pen.DashPattern = defaultDashPattern;
                        break;
                    case DashStyle.DashDot:
                        pen.DashPattern = defaultDashDotPattern;
                        break;
                    default:
                        pen.DashPattern = defaultDashDotDotPattern;
                        break;
                }
            }
            pens.Add(desc, pen);
            return pen;
        }

        public void Dispose()
        {
            foreach (var pair in pens)
            {
                pair.Value.Dispose();
            }
            pens.Clear();
        }
    }
}
