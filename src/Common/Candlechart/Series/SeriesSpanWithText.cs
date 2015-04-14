using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    public class SeriesSpanWithText : Series
    {
        public List<SpanWithText> data = new List<SpanWithText>();
        public override int DataCount { get { return data.Count; } }
        public bool StretchYAxis { get; set; }

        public SeriesSpanWithText(string name)
            : base(name)
        {
        }
        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            if (!StretchYAxis || data.Count == 0) return false;

            bool extends = false;
            float max = float.MinValue, min = float.MaxValue;
            foreach (var span in data)
            {
                if (span.EndIndex < left || span.StartIndex > right) continue;
                if (span.Price > max) max = span.Price;
                if (span.Price < min) min = span.Price;
            }            
            
            if (max > top)
            {
                top = max;
                extends = true;
            }
            if (min < bottom)
            {
                bottom = min;
                extends = true;
            }            

            return extends;            
        }

        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            if (data.Count == 0) return;
            base.Draw(g, worldRect, canvasRect);            
            using (var pens = new PenStorage())
            using (var brushes = new BrushesStorage())
            {                
                foreach (var span in data)
                {
                    span.Draw(g, pens, brushes, worldRect, canvasRect, Chart.Font);
                }
            }            
        }

        public bool ProcessMouseDown(int x, int y)
        {
            //foreach (var span in data)
            //{
            //}            
            return false;
        }
    }

    /// <summary>
    /// ортогональный отрезок, с одной либо с двух сторон дополненный текстом
    /// </summary>
    public class SpanWithText : IChartInteractiveObject
    {
        [Browsable(false)]
        public string ClassName { get { return Localizer.GetString("TitleSegmentWithText"); } }

        public bool Selected { get; set; }
        
        public string Name { get; set; }

        public DateTime? DateStart { get { return null; } set { } }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public float Price { get; set; }

        public string TextLeft { get; set; }

        public string TextRight { get; set; }

        private Color lineColor = Color.Black;
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private float lineWidth = 1;
        public float LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }
        
        public int IndexStart
        {
            get { return StartIndex; }
        }

        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }

        public void Draw(Graphics g, PenStorage pens, BrushesStorage brushes,
            RectangleD worldRect, Rectangle canvasRect, Font font)
        {
            var ptA = Conversion.WorldToScreen(new PointD(StartIndex, Price),
                    worldRect, canvasRect);
            var ptB = Conversion.WorldToScreen(new PointD(EndIndex, Price),
                worldRect, canvasRect);
            // линия
            var pen = pens.GetPen(LineColor, LineWidth);
            g.DrawLine(pen, (float)ptA.X, (float)ptA.Y, (float)ptB.X, (float)ptB.Y);
            // текст
            if (string.IsNullOrEmpty(TextLeft) && string.IsNullOrEmpty(TextRight)) return;
            
            const int textOffsetPx = 2;
            var brush = brushes.GetBrush(LineColor);
            var fmt = new StringFormat
                       { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            if (!string.IsNullOrEmpty(TextLeft))
                g.DrawString(TextLeft, font, brush, (float) ptA.X - textOffsetPx,
                                (float) ptA.Y, fmt);
            if (!string.IsNullOrEmpty(TextRight))
            {
                fmt.Alignment = StringAlignment.Near;
                g.DrawString(TextRight, font, brush, (float) ptB.X + textOffsetPx,
                                (float) ptB.Y, fmt);
            }            
        }

        public void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var newNode = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("SpanWithText"));

            newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();

            var atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("start"));
            atr.Value = StartIndex.ToString();

            atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("end"));
            atr.Value = EndIndex.ToString();

            atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("price"));
            atr.Value = Price.ToStringUniform();

            atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("tstLeft"));
            atr.Value = TextLeft;

            atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("tstRight"));
            atr.Value = TextRight;

            if (!string.IsNullOrEmpty(Name))
            {
                atr = newNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
                atr.Value = Name;
            }
        }

        public void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            if (itemNode.Attributes["start"] != null)
                StartIndex = itemNode.Attributes["start"].Value.ToInt();
            if (itemNode.Attributes["end"] != null)
                EndIndex = itemNode.Attributes["end"].Value.ToInt();
            if (itemNode.Attributes["price"] != null)
                Price = itemNode.Attributes["price"].Value.ToFloatUniform();
            if (itemNode.Attributes["tstLeft"] != null)
                TextLeft = itemNode.Attributes["tstLeft"].Value;
            if (itemNode.Attributes["tstRight"] != null)
                TextRight = itemNode.Attributes["TextRight"].Value;
        }

        public ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys)
        {
            return null;
        }

        public void OnMarkerMoved(ChartObjectMarker marker)
        {            
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
        }

        public Image CreateSample(Size sizeHint)
        {
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            var g = Graphics.FromImage(result);
            var pens = new PenStorage();
            var brushes = new BrushesStorage();
            var worldRect = new RectangleD(StartIndex - 1, Price * 0.9, EndIndex + 1, Price * 1.1);
            Draw(g, pens, brushes, worldRect, new Rectangle(new Point(0, 0), sizeHint),
                 new Font(FontFamily.GenericSansSerif, 8));
            return result;
        }
    }
}
