using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Candlechart.ChartMath;
using TradeSharp.Util;

namespace Candlechart.Core
{
    public class InfoTool : InteractivityTool
    {
        public InfoTool(string name)
        {
            Name = name;
        }
        internal override bool HandleMouseMove(MouseEventArgs e)
        {
            if (Owner.Owner.ItemTextBox == null) return false;
            // перевод координат куррсора в координаты графика
            PointD chartPoint = GetStockPanePoint(e.X, e.Y);            
            // цена в точке
            var data = new StringBuilder();
            data.AppendLine(PriceUnderCursor(chartPoint));
            // данные свечки
            int trend;
            data.AppendLine(CandleData(chartPoint, out trend));
            Owner.Owner.ItemTextBox.Text = data.ToString();
            var targetTextColor = GetColorByTrend(trend);
            if (Owner.Owner.ItemTextBox.ForeColor != targetTextColor)
                Owner.Owner.ItemTextBox.ForeColor = targetTextColor;
            return false;
        }

        private static Color GetColorByTrend(int trend)
        {
            if (trend == 0) return Color.Black;
            return trend < 0 ? Color.DarkSalmon : Color.DarkBlue;
        }

        private static string PriceUnderCursor(PointD chartPoint)
        {
            return Math.Round(chartPoint.Y, 4).ToString();
        }
        
        private string CandleData(PointD chartPoint, out int trend)
        {
            trend = 0;
            int index = (int)(chartPoint.X + 0.5);
            if (index < 0 || index >= Owner.Owner.StockSeries.Data.Count)
                return "";

            float open = Owner.Owner.StockSeries.Data[index].open;
            float close = Owner.Owner.StockSeries.Data[index].close;
            if (chartPoint.Y > Math.Max(open, close) || chartPoint.Y < Math.Min(open, close))
                return "";

            float high = Owner.Owner.StockSeries.Data[index].high;
            float low = Owner.Owner.StockSeries.Data[index].low;
            DateTime date = Owner.Owner.StockSeries.Data[index].timeOpen;
            trend = open < close ? 1 : -1;            
            string dateStr = date.ToString("dd.MM.yyyy HH:mm");
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("O: {0}", open.ToStringUniformPriceFormat(true)));
            sb.AppendLine(string.Format("H: {0}", high.ToStringUniformPriceFormat(true)));
            sb.AppendLine(string.Format("L: {0}", low.ToStringUniformPriceFormat(true)));
            sb.AppendLine(string.Format("C: {0}", close.ToStringUniformPriceFormat(true)));
            sb.AppendLine(dateStr);
            return sb.ToString();
        }        

        private PointD GetStockPanePoint(int x, int y)
        {
            var panePoint = new Point(x, y);
            return Conversion.ScreenToWorld(new PointD(panePoint.X, panePoint.Y),
               Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);
        }
    }    
}