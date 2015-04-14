using System.Drawing;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Entity;

namespace Candlechart.Core
{
    public delegate void KeyTurnBarSelected(CandleData bar);
    public class TurnDayTool : InteractivityTool
    {
        public event KeyTurnBarSelected KeyTurnBarSelected;

        public TurnDayTool(string _name)
        {
            Name = _name;
        }        
        
        //internal override bool HandleMouseDown(MouseEventArgs e)
        public bool OnMouseDown(MouseEventArgs e)
        {
            if (e.Clicks < 2) return false;
            PointD chartPoint = GetStockPanePoint(e.X, e.Y);
            double index = FindDataIndex(chartPoint);
            var candle = GetCandleData((int)index);
            if (candle == null) return false;
            // поставить разворотный бар
            if (KeyTurnBarSelected != null)
                KeyTurnBarSelected(candle);
            return false;
        }

        private CandleData GetCandleData(int index)
        {
            if (index < 0 || index >= Owner.Owner.StockSeries.Data.Count)
                return null;
            return Owner.Owner.StockSeries.Data[index];                
        }

        private static double FindDataIndex(PointD chartPoint)
        {
            return chartPoint.X + 0.5;
        }

        private PointD GetStockPanePoint(int x, int y)
        {
            var panePoint = new Point(x, y);
            return Conversion.ScreenToWorld(new PointD(panePoint.X, panePoint.Y),
               Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);
        }
    }    
}
