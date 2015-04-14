using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Candlechart.ChartMath;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class PolygonSeries : Series
    {
        public Color colorCloudA = Color.Red;
        public Color colorCloudB = Color.RoyalBlue;

        public struct DoubleLinePoint
        {
            public double x;
            public double yA;
            public double yB;
        }

        private double singleBarRegionWidth = 0.5d;
        public double SingleBarRegionWidth
        {
            get { return singleBarRegionWidth; }
            set { singleBarRegionWidth = value; }
        }

        private const int RegionAlpha = 35;

        public List<DoubleLinePoint> data = new List<DoubleLinePoint>();

        public override int DataCount { get { return data.Count; } }

        public PolygonSeries(string name) : base(name)
        {
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {
            return false;
        }
        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            DrawPolygons(g, worldRect, canvasRect);
        }

        private void DrawPolygons(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            if (worldRect.Width < 2) return;

            var start = (int)worldRect.X < 0 ? 0 : (int)worldRect.X;
            var end = (int)(worldRect.Right + 0.5);

            if (end >= data.Count) end = data.Count;
            if (start >= end) return;

            var currentPoligonPointTop = new List<PointD>();
            var currentPoligonPointBotton = new List<PointD>();
            var senkouMode = data[start].yA > data[start].yB;

            for (var i = 0; i < data.Count; i++)
            {
                if (data[i].x < worldRect.Left) continue;
                if (data[i].x > worldRect.Right) break;

                var currentSenkouMode = data[i].yA > data[i].yB;

                if (senkouMode != currentSenkouMode)
                {
                    // "Собираем" предыдущий полигон
                    AddPoligon(g, worldRect, canvasRect, currentPoligonPointBotton, currentPoligonPointTop, senkouMode);

                    // Начинаем собирать точки нового полигона 
                    currentPoligonPointTop = new List<PointD>();
                    currentPoligonPointBotton = new List<PointD>();

                    senkouMode = currentSenkouMode;
                }
                currentPoligonPointTop.Add(new PointD(data[i].x, data[i].yA));
                currentPoligonPointBotton.Add(new PointD(data[i].x, data[i].yB));
            }
            AddPoligon(g, worldRect, canvasRect, currentPoligonPointBotton, currentPoligonPointTop, senkouMode);
        }

        private void AddPoligon(Graphics g, RectangleD worldRect, Rectangle canvasRect, List<PointD> currentPoligonPointBotton,
                                       List<PointD> currentPoligonPointTop, bool currentSenkouMode)
        {
            currentPoligonPointBotton.Reverse();
            currentPoligonPointTop.AddRange(currentPoligonPointBotton);
            var pountsF = currentPoligonPointTop.Select(x => Conversion.WorldToScreen(x, worldRect, canvasRect).ToPointF()).ToArray();

            if (pountsF.Length > 0)
            {
                var brush = currentSenkouMode
                                ? new SolidBrush(Color.FromArgb(RegionAlpha, colorCloudA))
                                : new SolidBrush(Color.FromArgb(RegionAlpha, colorCloudB));
                g.FillPolygon(brush, pountsF);
            }
        }
    }
}
