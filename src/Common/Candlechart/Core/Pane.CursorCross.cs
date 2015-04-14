using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Core
{
    public partial class Pane
    {
        private static readonly float[] dashPattern = new float[] { 2, 2 };

        public bool ShowCrossToolTip { get; set; }

        /// <summary>
        /// true если координаты перекрестия попадают в наблюдаемую область
        /// </summary>
        private bool cursorInBounds;
        
        private PointD? cursorCrossCoords;
        /// <summary>
        /// модельные координаты перекрестия, м.б. null
        /// </summary>
        public PointD? CursorCrossCoords
        {
            get { return cursorCrossCoords; }
            set
            {
                if (cursorCrossCoords == value) return;
                var inBounds = value == null
                                   ? false
                                   : (WorldRect.Left <= value.Value.X &&
                                      WorldRect.Right >= value.Value.X);
                var needInvalidate = inBounds || cursorInBounds;
                cursorInBounds = inBounds;

                cursorCrossCoords = value;
                Chart.XAxis.SelectedLabelX = value.HasValue ? value.Value.X : (double?)null;
                yAxis.SelectedLabelPrice = value.HasValue ? value.Value.Y : (double?)null;

                if (needInvalidate) Chart.Invalidate();
            }
        }

        private void DrawCursorCross(Graphics gr, BrushesStorage brushes)
        {
            if (!cursorInBounds) return;

            var pointModel = cursorCrossCoords;
            var pointScreen = Conversion.WorldToScreen(pointModel.Value,
                WorldRect, CanvasRect);
            var pointRight = new PointD(CanvasRect.Width, CanvasRect.Height);

            // нарисовать перекрестие
            using (var penCross = new Pen(Chart.visualSettings.SeriesForeColor)
                                      {
                                          DashStyle = DashStyle.Custom,
                                          DashPattern = dashPattern
                                      })
            {
                gr.DrawLine(penCross, 0, (int) pointScreen.Y, (int) pointRight.X, (int) pointScreen.Y);
                gr.DrawLine(penCross, (int) pointScreen.X, 0, (int) pointScreen.X, Height);
            }
            if (!ShowCrossToolTip) return;

            // нарисовать tooltip
            var tipText = Chart.GetTipText((int) pointScreen.X, (int) pointScreen.Y);
            if (!string.IsNullOrEmpty(tipText))            
                DrawHint(gr, pointScreen, tipText, brushes);
        }

        private void DrawHint(Graphics gr, PointD pointScreen, 
            string tipText, BrushesStorage brushes)
        {
            var spaceToRight = CanvasRect.Width - pointScreen.X;
            var spaceToBotom = CanvasRect.Height - pointScreen.Y;
            var tipSz = gr.MeasureString(tipText, Chart.Font);

            const int textPadding = 2;
            const int pointDisplace = 2;
            tipSz.Width += textPadding*2;
            tipSz.Height += textPadding*2;

            var placeRight = spaceToRight >= tipSz.Width;
            var placeBotom = spaceToBotom >= tipSz.Height;

            var ptLeft = new Point((int) pointScreen.X + pointDisplace,
                                   (int) pointScreen.Y + pointDisplace);
            if (!placeRight) ptLeft.X -= (pointDisplace*2 + (int) tipSz.Width);
            if (!placeBotom) ptLeft.Y -= (pointDisplace*2 + (int) tipSz.Height);

            var rect = new Rectangle(ptLeft, new Size((int) tipSz.Width, (int) tipSz.Height));
            using (var brushFill = new SolidBrush(Chart.visualSettings.PaneBackColor))
                gr.FillRectangle(brushFill, rect);
            using (var pen = new Pen(Chart.visualSettings.SeriesForeColor))
                gr.DrawRectangle(pen, rect);
            using (var brushText = new SolidBrush(Chart.visualSettings.SeriesForeColor))
            {//brushes.GetBrush();
                try
                {
                    gr.DrawString(tipText, Chart.Font, brushText, ptLeft);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("DrawHint::DrawString error (left is {0}) - {1}", ptLeft, ex);
                    //throw;
                }
            }
        }
    }
}
