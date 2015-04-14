using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;

namespace Candlechart.Series
{
    public partial class ChartComment
    {
        public void DrawComment(
            Graphics g, FontStorage fonts,
            RectangleD worldRect, Rectangle canvasRect, PenStorage penStorage,
            BrushesStorage brushStorage,
            List<Rectangle> areasToCheck)
        {
            var oldMode = g.SmoothingMode;
            g.SmoothingMode = /*SmoothingMode.AntiAlias : */SmoothingMode.None;
            try
            {
                var brush = brushStorage.GetBrush(Color.FromArgb(FillTransparency, ColorFill));

                // нормализовать угол стрелки
                var arrowAngle = GetNormalizedArrowAngle();
                while (arrowAngle < 0)
                    arrowAngle = Math.PI * 2 + arrowAngle;
                // точка привязки
                PointD ptPivot = Conversion.WorldToScreen(new PointD(PivotIndex, PivotPrice),
                                                            worldRect, canvasRect);
                // вторая точка стрелки
                var ptEnd = new PointD(ptPivot.X + ArrowLength * Math.Cos(arrowAngle),
                                        ptPivot.Y + ArrowLength * Math.Sin(arrowAngle));

                var commentText = string.IsNullOrEmpty(Text) ? "" : Text;
                var color = Color;
                commentText = GetColorByComment(commentText, ref color);
                ReplaceTextPatterns();

                var dashStyle = IsBeingCreated ? DashStyle.Dash : DashStyle.Solid;
                var pen = penStorage.GetPen(color,
                    Selected ? 3f : 1f, dashStyle);

                // посчитать координаты текста                                        
                float top, left, width, height;
                GetTextAreaLeftTop(g, fonts, commentText, arrowAngle, ptEnd,
                    out left, out top, out width, out height);

                // проверить пересечение с комментом
                if (areasToCheck != null)
                {
                    var rectOwn = new Rectangle((int)top, (int)left, (int)width, (int)height);
                    if (areasToCheck.Any(a => a.IntersectsWith(rectOwn)))
                        return;
                    areasToCheck.Add(rectOwn);
                }

                // нарисовать область с текстом
                if (!HideBox)
                    DrawTextArea(pen, g, brush, left, top, width, height);

                // .. и сам текст
                DrawTextOrSymbol(commentText, brushStorage, fonts, ColorText, g, left, top, width, height);
                TextArea = new Rectangle((int)left + canvasRect.Left, (int)top, (int)width, (int)height);

                // нарисовать стрелку
                if (!HideArrow)
                {
                    PointD pointArrowEnd;
                    List<PointF> arrowPoints = GetArrowPoints(arrowAngle, ptPivot, out pointArrowEnd);
                    // нарисовать палку
                    g.DrawLine(pen, (float)pointArrowEnd.X, (float)pointArrowEnd.Y, (float)ptEnd.X, (float)ptEnd.Y);
                    // нарисовать стрелочку
                    g.FillPolygon(brush, arrowPoints.ToArray());
                    g.DrawPolygon(pen, arrowPoints.ToArray());
                }

                // маркеры
                if (Selected)
                    DrawMarkers(g, worldRect, canvasRect);
            }
            finally
            {
                g.SmoothingMode = oldMode;
            }
        }

        /// <summary>
        /// вернуть массив точек стрелки
        /// </summary>
        /// <param name="arrowAngle">угол наклона самой линии (нормализованный)</param>
        /// <param name="ptPivot">конечная точка, от которой рисуется стрелка (острие)</param>
        /// <param name="arrowO">точка между "крыльями" стрелки</param>
        /// <returns>три точки стрелки</returns>
        private static List<PointF> GetArrowPoints(double arrowAngle, PointD ptPivot, out PointD arrowO)
        {
            // острие стрелки       
            const int arrowLen = 12;
            const double arrowAlpha = 12.25;
            arrowO = new PointD(ptPivot.X + Math.Cos(arrowAngle) * arrowLen,
                                    ptPivot.Y + Math.Sin(arrowAngle) * arrowLen);
            var arrowA = new PointF((float)(ptPivot.X + (arrowO.X - ptPivot.X) * Math.Cos(arrowAlpha) - (arrowO.Y - ptPivot.Y) * Math.Sin(arrowAlpha)),
                                    (float)(ptPivot.Y + (arrowO.X - ptPivot.X) * Math.Sin(arrowAlpha) + (arrowO.Y - ptPivot.Y) * Math.Cos(arrowAlpha)));
            var arrowB = new PointF((float)(ptPivot.X + (arrowO.X - ptPivot.X) * Math.Cos(-arrowAlpha) - (arrowO.Y - ptPivot.Y) * Math.Sin(-arrowAlpha)),
                                    (float)(ptPivot.Y + (arrowO.X - ptPivot.X) * Math.Sin(-arrowAlpha) + (arrowO.Y - ptPivot.Y) * Math.Cos(-arrowAlpha)));
            return new List<PointF> { arrowA, arrowB, new PointF((float)ptPivot.X, (float)ptPivot.Y) };
        }

        public RectangleF GetCommentRectangle(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            // точка привязки
            var ptPivot = Conversion.WorldToScreen(new PointD(PivotIndex, PivotPrice),
                                                        worldRect, canvasRect);
            // вторая точка стрелки
            var angle = GetNormalizedArrowAngle();
            var ptEnd = new PointD(ptPivot.X + ArrowLength * Math.Cos(angle),
                                    ptPivot.Y + ArrowLength * Math.Sin(angle));

            var fontSize = Owner == null ? 8 : SeriesComment.FontSize;
            var fontStyle = SeriesComment.FontBold ? FontStyle.Bold : FontStyle.Regular;

            using (var fonts = new FontStorage(new Font(FontFamily.GenericSansSerif, fontSize, fontStyle)))
            {
                float top, left, width, height;
                GetTextAreaLeftTop(g, fonts, Text, arrowAngle, ptEnd,
                                   out left, out top, out width, out height);
                return new RectangleF(left, top, width, height);
            }
        }

        /// <summary>
        /// в комментарии заменить вхождения шаблонов (типа [price]) на значения подстановки
        /// </summary>
        private void ReplaceTextPatterns()
        {
            if (string.IsNullOrEmpty(Text)) return;
            const string patternPrice = "[price]";
            const int patternPriceLen = 7;
            while (true)
            {
                var start = Text.IndexOf(patternPrice);
                if (start < 0) break;
                var priceStr = string.Format(" {0:f4} ", PivotPrice);
                Text = Text.Substring(0, start) + priceStr +
                               Text.Substring(start + patternPriceLen,
                                                      Text.Length - start - patternPriceLen);
            }
        }

        private void DrawTextArea(Pen pen, Graphics g, Brush brush,
            float left, float top, float width, float height)
        {
            g.FillRectangle(brush, left, top, width, height);
            if (DrawFrame)
                g.DrawRectangle(pen, left, top, width, height);
        }

        private void DrawTextOrSymbol(string comment,
            BrushesStorage brushes,
            FontStorage fonts,
            Color commentColor, Graphics g,
            float left, float top, float width, float height)
        {
            const int textShift = 4;
            if (comment != null)
                if (comment.Contains(TemplateBuy) || comment.Contains(TemplateSell))
                {
                    var smb = symbolBuySell.Copy();
                    smb.Scale(SymbolBuySellWidth / smb.Width, SymbolBuySellHeight / smb.Height);
                    if (comment.Contains(TemplateBuy))
                    {
                        smb.Move2Point(new PointF(left + width / 2, top + height - textShift));
                        smb.PaintFills(Color.Blue);
                    }
                    if (comment.Contains(TemplateSell))
                    {
                        smb.Rotate((float)-Math.PI);
                        smb.Move2Point(new PointF(left + width / 2, top + textShift));
                        smb.PaintFills(Color.Red);
                    }
                    smb.Draw(g);
                    return;
                }
            var text = string.IsNullOrEmpty(comment) ? "<пусто>" : comment;
            g.DrawFormattedText(left + textShift, top + textShift, brushes, commentColor,
                text, fonts);
        }

        /// <summary>
        /// определить координаты рамки с текстом, получив на входе
        /// нормализованный угол линии и конечную точку
        /// </summary>
        private void GetTextAreaLeftTop(Graphics g, FontStorage font,
            string commentText, double arrowAngle, PointD ptEnd,
            out float left, out float top, out float width, out float height)
        {
            const int textShift = 7;
            var textSz = MeasureCommentBlock(g, font, commentText);
            textSz.Width += textShift * 2;
            textSz.Height += textShift * 2;
            width = textSz.Width;
            height = textSz.Height;

            const double PI4 = .785398, PI34 = 2.356194, PI54 = 3.926991, PI74 = 5.497787;
            if (arrowAngle >= PI4 && arrowAngle < PI34)
            {// квадрат снизу
                left = (float)ptEnd.X - textSz.Width / 2;
                top = (float)ptEnd.Y;
            }
            else if (arrowAngle >= PI34 && arrowAngle < PI54)
            {// квадрат слева
                left = (float)ptEnd.X - textSz.Width;
                top = (float)ptEnd.Y - textSz.Height / 2;
            }
            else if (arrowAngle >= PI54 && arrowAngle < PI74)
            {// квадрат сверху
                left = (float)ptEnd.X - textSz.Width / 2;
                top = (float)ptEnd.Y - textSz.Height;
            }
            else
            {// квадрат справа
                left = (float)ptEnd.X;
                top = (float)ptEnd.Y - textSz.Height / 2;
            }
        }

        public SizeF MeasureCommentBlock(Graphics g, FontStorage fonts, string commentText)
        {
            if (commentText != null)
                if (commentText.Contains(TemplateBuy) || commentText.Contains(TemplateSell))
                    return new SizeF(SymbolBuySellWidth, SymbolBuySellHeight);
            var text = string.IsNullOrEmpty(commentText) ? "<пусто>" : commentText;
            return g.MeasureFormattedText(text, fonts);
        }

        /// <summary>
        /// если в тексте встретилось: {color:Red}, {color:#FF0010}...
        /// убрать это вхождение и изменить цвет коментария
        /// вернуть текст без вхождения
        /// </summary>
        private string GetColorByComment(string comment, ref Color commentColor)
        {
            var i = comment.IndexOf("{color:");
            if (i < 0) return comment;
            var lastIndex = comment.IndexOf('}', i);
            if (lastIndex < 0) return comment;
            var colorValue = comment.Substring(i + 7, lastIndex - i - 7);
            var cl = Color.FromName(colorValue);
            if (cl.ToArgb() == 0)
            {
                try
                {
                    cl = Color.FromArgb(int.Parse(colorValue.Substring(1, 2)),
                                        int.Parse(colorValue.Substring(3, 2)), int.Parse(colorValue.Substring(5, 2)));
                }
                catch
                {
                    cl = Color.Black;
                }
            }
            commentColor = cl;
            return string.Format("{0}{1}", comment.Substring(0, i),
                                 lastIndex < (comment.Length - 1) ? comment.Substring(lastIndex + 1) : "");
        }

        public RectangleF GetCommentRectangle(Graphics g, FontStorage fonts, RectangleD worldRect, Rectangle canvasRect)
        {
            // точка привязки
            var ptPivot = Conversion.WorldToScreen(new PointD(PivotIndex, PivotPrice),
                                                        worldRect, canvasRect);
            // вторая точка стрелки
            var arrowAngle = GetNormalizedArrowAngle();
            var ptEnd = new PointD(ptPivot.X + ArrowLength * Math.Cos(arrowAngle),
                                    ptPivot.Y + ArrowLength * Math.Sin(arrowAngle));
            float top, left, width, height;
            GetTextAreaLeftTop(g, fonts, Text, arrowAngle, ptEnd,
                                out left, out top, out width, out height);
            return new RectangleF(left, top, width, height);
        }

        /// <summary>
        /// из градусов в радианы от 0 до 2*PI
        /// </summary>
        private double GetNormalizedArrowAngle()
        {
            var arrowAngle = ArrowAngle / 180 * Math.PI;
            while (arrowAngle < 0)
                arrowAngle = Math.PI * 2 + arrowAngle;
            return arrowAngle;
        }
    }
}
