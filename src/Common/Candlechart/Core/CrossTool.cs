using System;
using System.Drawing;
using System.Windows.Forms;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    public class CrossTool : InteractivityTool
    {
        private MouseButtons mouseButton = MouseButtons.Middle;

        private bool draggingIsOn;
        
        public MouseButtons MouseButton
        {
            get { return mouseButton; }
            set { mouseButton = value; }
        }
        
        public bool ShowTooltip { get; set; }

        public CrossTool(string name)
        {
            Name = name;
        }

        internal override bool HandleMouseDown(MouseEventArgs e)
        {
            if ((e.Button & mouseButton) != mouseButton) return false;
            
            var modelCoords = GetStockPanePoint(e.X, e.Y);
            Chart.StockPane.CursorCrossCoords = modelCoords;
            Chart.StockPane.ShowCrossToolTip = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
            RaiseCursorCrossChangedEvent(modelCoords);

            draggingIsOn = true;
            return true;
        }

        internal override bool HandleMouseMove(MouseEventArgs e)
        {
            if (!draggingIsOn) return false;
            var modelCoords = GetStockPanePoint(e.X, e.Y);
            Chart.StockPane.CursorCrossCoords = modelCoords;
            RaiseCursorCrossChangedEvent(modelCoords);
            return true;
        }

        internal override bool HandleMouseUp(MouseEventArgs e)
        {
            if (!draggingIsOn) return false;
            draggingIsOn = false;
            Chart.StockPane.CursorCrossCoords = null;
            RaiseCursorCrossChangedEvent(null);

            return true;
        }
        
        private PointD GetStockPanePoint(int x, int y)
        {
            var panePoint = new Point(x, y);
            return Conversion.ScreenToWorld(new PointD(panePoint.X, panePoint.Y),
               Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);
        }
    
        /// <summary>
        /// оповестить другие графики
        /// </summary>
        private void RaiseCursorCrossChangedEvent(PointD? crossCoords)
        {
            DateTime? time = null;
            double? price = null;
            if (crossCoords.HasValue)
            {
                price = crossCoords.Value.Y;
                var x = (int) (crossCoords.Value.X + 0.5);
                if (x >= 0 && x < Chart.StockSeries.Data.Count)
                    time = Chart.StockSeries.Data.Candles[x].timeOpen;
            }
            Chart.Owner.onCursorCrossUpdated(time, price, Chart.Owner);
        }
    }
}
