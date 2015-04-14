using System;
using System.Drawing;
using System.Text;

namespace Entity
{
    public static class GraphicsExtensions
    {
        #region Прямоугольник со скругленными углами
        private static readonly PointF[] roundPoints3 =
            new[] { new PointF(0, 1), new PointF(-0.7071F, 0.7071F), new PointF(-1, 0) };
        private static readonly PointF[] roundPoints4 =
            new[] { new PointF(0, 1), new PointF(-0.5F, 0.866F), new PointF(-0.866F, 0.5F), new PointF(-1, 0) };
        private static readonly PointF[] roundPoints5 =
            new[] { new PointF(0, 1), new PointF(-0.3827F, 0.9239F), new PointF(-0.7071F, 0.7071F), 
                new PointF(-0.9239F, 0.3827F), new PointF(-1, 0) };
        private static readonly PointF[] roundPoints6 =
            new[] { new PointF(0, 1), new PointF(-0.2588F, 0.9659F), new PointF(-0.5F, 0.866F), 
                new PointF(-0.7071F, 0.7071F), new PointF(-0.866F, 0.5F), new PointF(-0.9659F, 0.2588F), new PointF(-1, 0) };        

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle r, int rad)
        {
            if (rad < 1)
            {
                graphics.DrawRectangle(pen, r);
                return;
            }
            if (rad == 1)
            {
                graphics.DrawLine(pen, r.Left + 1, r.Top, r.Right - 1, r.Top);
                graphics.DrawLine(pen, r.Right, r.Top + 1, r.Right, r.Bottom - 1);
                graphics.DrawLine(pen, r.Left + 1, r.Bottom, r.Right - 1, r.Bottom);
                graphics.DrawLine(pen, r.Left, r.Top + 1, r.Left, r.Bottom - 1);
                return;
            }
            var pointsOfArc = rad < 6 ? roundPoints3 : rad < 15 ? roundPoints4 : rad < 25 ? roundPoints5 : roundPoints6;
            var points = MakeRoundedRectanglePoints(r, rad, pointsOfArc);
            graphics.DrawPolygon(pen, points);
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle r, int rad)
        {
            if (rad < 2)
            {
                graphics.FillRectangle(brush, r);
                return;
            }
            var pointsOfArc = rad < 5 ? roundPoints3 : rad < 9 ? roundPoints4 : rad < 15 ? roundPoints5 : roundPoints6;
            var points = MakeRoundedRectanglePoints(r, rad, pointsOfArc);
            graphics.FillPolygon(brush, points);
        }

        private static PointF[] MakeRoundedRectanglePoints(Rectangle r, int rad,
            PointF[] pointsOfArc)
        {
            pointsOfArc = roundPoints6;

            var pointsPoly = new PointF[pointsOfArc.Length * 4];
            var pointIndex = 0;

            var centers = new[]
                              {
                                  new PointF(r.Left + rad, r.Top + rad),
                                  new PointF(r.Left + rad, r.Bottom - rad), 
                                  new PointF(r.Right - rad, r.Bottom - rad),
                                  new PointF(r.Right - rad, r.Top + rad)
                              };
            for (var i = 0; i < centers.Length; i++)
            {
                foreach (var scale in pointsOfArc)
                {
                    var scaleX = i == 0 ? scale.X : i == 1 ? -scale.Y : i == 2 ? -scale.X : scale.Y;
                    var scaleY = i == 0 ? -scale.Y : i == 1 ? -scale.X : i == 2 ? scale.Y : scale.X;
                    pointsPoly[pointIndex++] = new PointF(centers[i].X + scaleX * rad,
                        centers[i].Y + scaleY * rad);
                }
            }

            return pointsPoly;
        }
        #endregion

        #region Рисование форматированного текста
        private static readonly char[] lineSeparator = new[] { '\n' };
        /// <summary>
        /// для простоты - каждая строка текста имеет свой формат
        /// необязательные модификаторы вида [b][i][u][#RRGGBB] идут _в начале_ строки
        /// [b][i][u][#ff00e6]This is a bold/italic/underlined magenta line of text
        /// </summary>
        public static SizeF MeasureFormattedText(this Graphics g, string text, FontStorage fonts)
        {
            if (string.IsNullOrEmpty(text)) return new SizeF(0, 0);
            var lines = text.Split(lineSeparator, StringSplitOptions.RemoveEmptyEntries);

            var totalHeight = 0f;
            var totalWidth = 0f;

            foreach (var line in lines)
            {
                FontStyle fontStyle;
                Color? fontColor;

                var pureLine = GetLineModifiers(line, out fontStyle, out fontColor);
                var font = fonts.GetFont(fontStyle);
                var dH = font.GetHeight(g);
                totalHeight += dH;

                if (string.IsNullOrEmpty(pureLine)) continue;
                var sz = g.MeasureString(pureLine, font);
                if (sz.Width > totalWidth) totalWidth = sz.Width;
            }

            return new SizeF(totalWidth, totalHeight);
        }

        public static void DrawFormattedText(this Graphics g,
            float x, float y,
            BrushesStorage brushes,
            Color colorMain,
            string text, FontStorage fonts)
        {
            if (string.IsNullOrEmpty(text)) return;
            var lines = text.Split(lineSeparator, StringSplitOptions.RemoveEmptyEntries);
            var totalHeight = 0f;

            foreach (var line in lines)
            {
                FontStyle fontStyle;
                Color? fontColor;

                var pureLine = GetLineModifiers(line, out fontStyle, out fontColor);
                var font = fonts.GetFont(fontStyle);

                g.DrawString(pureLine, font, brushes.GetBrush(fontColor ?? colorMain), x, y + totalHeight);

                var dH = font.GetHeight(g);
                totalHeight += dH;
            }
        }

        public static string GetLineModifiers(string line, out FontStyle style, out Color? color)
        {
            // [b][i][u][#ff00e6]This is a bold/italic/underlined magenta line of text
            style = FontStyle.Regular;
            color = null;
            if (string.IsNullOrEmpty(line)) return line;
            
            while (true)
            {
                if (line.StartsWith("[b]"))
                {
                    line = line.Substring("[b]".Length);
                    style = style | FontStyle.Bold;
                    continue;
                }
                if (line.StartsWith("[i]"))
                {
                    line = line.Substring("[i]".Length);
                    style = style | FontStyle.Italic;
                    continue;
                }
                if (line.StartsWith("[u]"))
                {
                    line = line.Substring("[u]".Length);
                    style = style | FontStyle.Underline;
                    continue;
                }
                if (line.StartsWith("["))
                {
                    var closeIndex = line.IndexOf(']');
                    if (closeIndex < 0) break;
                    var len = closeIndex - 1;
                    if (len == 0) break;

                    var colorStr = line.Substring(1, len);
                    try
                    {
                        color = ColorTranslator.FromHtml(colorStr);
                    }
                    catch
                    {
                        break;
                    }
                    line = closeIndex == (line.Length - 1) ? "" : line.Substring(closeIndex + 1);
                    continue;
                }
                break;
            }

            return line;
        }

        public static string FormatLine(string line, FontStyle style, Color? color)
        {
            var sb = new StringBuilder();
            if (color.HasValue)
                sb.AppendFormat("[{0}]", ColorTranslator.ToHtml(color.Value));
            if ((style & FontStyle.Bold) == FontStyle.Bold)
                sb.Append("[b]");
            if ((style & FontStyle.Italic) == FontStyle.Italic)
                sb.Append("[i]");
            if ((style & FontStyle.Underline) == FontStyle.Underline)
                sb.Append("[u]");
            sb.Append(line);
            return sb.ToString();
        }
        #endregion

        #region Работа с цветом
        public static void InvertColorLightness(ref byte r, ref byte g, ref byte b)
        {
            var lightness = Color.FromArgb(r, g, b).GetBrightness();
            int red = 255, green = 255, blue = 255;
            if (lightness > 0)
            {
                var newLightness = (1 - lightness) / lightness;
                red = (int)((r + 1) * newLightness);
                green = (int)((g + 1) * newLightness);
                blue = (int)((b + 1) * newLightness);
            }
            r = red > 255 ? (byte)255 : (byte)red;
            g = green > 255 ? (byte)255 : (byte)green;
            b = blue > 255 ? (byte)255 : (byte)blue;
        }
        #endregion
    }    
}
